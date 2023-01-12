#nullable enable

using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.AutoNumberAttribute
{
	public class DacAutoNumberAttributeAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType, 
				Descriptors.PX1020_InsufficientStringLengthForDacPropertyWithAutoNumbering 
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var autoNumberProperties = dacOrDacExt.DeclaredDacProperties.Where(property => property.IsAutoNumbering);
			AcumaticaAttributesInfoProvider attributesInfoProvider = new AcumaticaAttributesInfoProvider(pxContext);

			foreach (DacPropertyInfo dacProperty in autoNumberProperties)
			{
				CheckDacProperty(context, attributesInfoProvider, dacProperty);
			}		
		}

		private void CheckDacProperty(SymbolAnalysisContext context, AcumaticaAttributesInfoProvider attributesInfoProvider, DacPropertyInfo dacProperty)
		{
			if (dacProperty.PropertyType.SpecialType != SpecialType.System_String)
			{
				ReportDacPropertyTypeIsNotString(context, attributesInfoProvider.PxContext, dacProperty);
				return;
			}

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckIfStringLengthIsSufficientForAutoNumbering(context, attributesInfoProvider, dacProperty);
		}

		private void ReportDacPropertyTypeIsNotString(SymbolAnalysisContext context, PXContext pxContext, DacPropertyInfo dacProperty)
		{
			var autoNumberingAttribute = dacProperty.Attributes.FirstOrDefault(a => a.IsAutoNumberAttribute);
			var propertyTypeLocation = dacProperty.Node.Type.GetLocation();

			if (propertyTypeLocation != null)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType, propertyTypeLocation);

				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}

			var attributeLocation = autoNumberingAttribute?.AttributeData.GetLocation(context.CancellationToken);
			
			if (attributeLocation != null)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType, attributeLocation);

				context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}

		private void CheckIfStringLengthIsSufficientForAutoNumbering(SymbolAnalysisContext context, AcumaticaAttributesInfoProvider attributesInfoProvider,
																	 DacPropertyInfo dacProperty)
		{
			var dbBoundStringAttribute = attributesInfoProvider.PxContext.FieldAttributes.PXDBStringAttribute;
			var unboundStringAttribute = attributesInfoProvider.PxContext.FieldAttributes.PXStringAttribute;
			var stringAttributes = dacProperty.Attributes
											  .Where(a => IsStringAttribute(a, attributesInfoProvider, dbBoundStringAttribute, unboundStringAttribute))
											  .ToList();
			if (stringAttributes.Count != 1)
				return;

			AttributeInfo stringAttribute = stringAttributes[0];
			int? stringLength = GetStringLengthFromStringAttribute(stringAttribute);
			int minAllowedLength = attributesInfoProvider.PxContext.AttributeTypes.AutoNumberAttribute.MinAutoNumberLength;

			if (stringLength.HasValue && stringLength < minAllowedLength)
			{
				var attributeLocation = GetLocationToReportInsufficientStringLength(context, stringAttribute, stringLength.Value);

				if (attributeLocation == null) 
					return;

				var diagnostic = Diagnostic.Create(Descriptors.PX1020_InsufficientStringLengthForDacPropertyWithAutoNumbering, attributeLocation, minAllowedLength);
				context.ReportDiagnosticWithSuppressionCheck(diagnostic, attributesInfoProvider.PxContext.CodeAnalysisSettings);
			}
		}

		private bool IsStringAttribute(AttributeInfo attribute, AcumaticaAttributesInfoProvider attributesInfoProvider, 
									   INamedTypeSymbol dbBoundStringAttribute, INamedTypeSymbol unboundStringAttribute) =>
			attribute.BoundType != BoundType.NotDefined &&
			(attributesInfoProvider.IsAttributeDerivedFromOtherAttribute(attribute.AttributeType, dbBoundStringAttribute) ||
			 attributesInfoProvider.IsAttributeDerivedFromOtherAttribute(attribute.AttributeType, unboundStringAttribute));

		private int? GetStringLengthFromStringAttribute(AttributeInfo stringAttribute)
		{
			if (stringAttribute.AttributeData.ConstructorArguments.IsDefaultOrEmpty)
				return null;

			int lengthArgsCandidatesCount = 0;
			int? stringLength = null;

			foreach (var arg in stringAttribute.AttributeData.ConstructorArguments)
			{
				if (arg.Kind == TypedConstantKind.Primitive  && arg.Value is int length)
				{
					stringLength = length;
					lengthArgsCandidatesCount++;

					//We don't have enough information to pick between several integer attribute constructor arguments, so we can't obtain string length.
					if (lengthArgsCandidatesCount > 1)	
						return null;
				}
			}

			return stringLength;
		}

		private static Location? GetLocationToReportInsufficientStringLength(SymbolAnalysisContext context, AttributeInfo stringAttribute, int stringLength)
		{
			var syntaxNode = stringAttribute.AttributeData.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);

			if (!(syntaxNode is AttributeSyntax attributeSyntaxNode))
				return stringAttribute.AttributeData.GetLocation(context.CancellationToken);

			var argumentsList = attributeSyntaxNode.ArgumentList.Arguments;
			
			for (int i = 0; i < argumentsList.Count; i++)
			{
				AttributeArgumentSyntax argumenNode = argumentsList[i];

				if (argumenNode.NameEquals != null)		//filter out attribute property argument setters like "IsDirty = true"
					continue;

				switch (argumenNode.Expression)
				{
					case LiteralExpressionSyntax literal 
					when literal.Kind() == SyntaxKind.NumericLiteralExpression && literal.Token.Value is int argLength && argLength == stringLength:
						return literal.GetLocation();

					case IdentifierNameSyntax namedConstant:
						return namedConstant.GetLocation();

					case MemberAccessExpressionSyntax memberAccess:
						return memberAccess.GetLocation();

					default:
						continue;
				}
			}

			return stringAttribute.AttributeData.GetLocation(context.CancellationToken);
		}
	}
}
