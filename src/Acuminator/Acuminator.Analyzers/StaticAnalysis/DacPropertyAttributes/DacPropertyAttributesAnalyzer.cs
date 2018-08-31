using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacPropertyAttributesAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty,
				Descriptors.PX1023_DacPropertyMultipleFieldAttributes
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(symbolContext => AnalyzePropertyAsync(symbolContext, pxContext), SymbolKind.NamedType);
		}

		private static Task AnalyzePropertyAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (!(symbolContext.Symbol is INamedTypeSymbol dacOrDacExt) || !dacOrDacExt.IsDacOrExtension(pxContext))
				return Task.FromResult(false);

			FieldAttributesRegister fieldAttributesRegister = new FieldAttributesRegister(pxContext);
			Task[] allTasks = dacOrDacExt.GetMembers()
										 .OfType<IPropertySymbol>()
										 .Select(property => CheckDacPropertyAsync(property, symbolContext, pxContext, fieldAttributesRegister))
										 .ToArray();

			return Task.WhenAll(allTasks);
		}

		private static async Task CheckDacPropertyAsync(IPropertySymbol property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														FieldAttributesRegister fieldAttributesRegister)
		{
			ImmutableArray<AttributeData> attributes = property.GetAttributes();

			if (attributes.Length == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var attributesWithInfo = GetFieldAttributesInfos(pxContext, attributes, fieldAttributesRegister, symbolContext.CancellationToken);

			if (symbolContext.CancellationToken.IsCancellationRequested || attributesWithInfo.IsNullOrEmpty())
				return;

			if (attributesWithInfo.Count > 1)
			{
				var locationTasks = attributesWithInfo.Select(info => GetAttributeLocationAsync(info.Attribute, symbolContext.CancellationToken));
				Location[] attributeLocations = await Task.WhenAll(locationTasks)
														  .ConfigureAwait(false);

				foreach (Location attrLocation in attributeLocations)
				{
					if (attrLocation == null)
						continue;

					symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1023_DacPropertyMultipleFieldAttributes, attrLocation));
				}
			}
			else
			{
				var (fieldAttribute, fieldAttrInfo) = attributesWithInfo[0];
				await CheckAttributeAndPropertyTypesForCompatibility(property, fieldAttribute, fieldAttrInfo, pxContext, symbolContext);
			}
		}

		private static List<(AttributeData Attribute, FieldAttributeInfo Info)> GetFieldAttributesInfos(PXContext pxContext,
																									   ImmutableArray<AttributeData> attributes,
																									   FieldAttributesRegister fieldAttributesRegister,
																									   CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var fieldInfosList = new List<(AttributeData, FieldAttributeInfo)>(capacity: attributes.Length);

			foreach (AttributeData attribute in attributes)
			{
				cancellationToken.ThrowIfCancellationRequested();
				FieldAttributeInfo attrInfo = fieldAttributesRegister.GetFieldAttributeInfo(attribute.AttributeClass);

				if (attrInfo.IsFieldAttribute)
					fieldInfosList.Add((attribute, attrInfo));
			}

			return fieldInfosList;
		}

		private static Task CheckAttributeAndPropertyTypesForCompatibility(IPropertySymbol property, AttributeData fieldAttribute,
																		   FieldAttributeInfo fieldAttributeInfo, PXContext pxContext,
																		   SymbolAnalysisContext symbolContext)
		{
			if (fieldAttributeInfo.FieldType == null)                                                               //PXDBFieldAttribute case
			{
				return CreateDiagnosticsAsync(property, fieldAttribute, symbolContext, registerCodeFix: false);
			}

			ITypeSymbol typeToCompare;

			if (property.Type.IsValueType)
			{
				if (!(property.Type is INamedTypeSymbol namedPropertyType))
					return null;

				typeToCompare = namedPropertyType.IsNullable(pxContext)
									? namedPropertyType.GetUnderlyingTypeFromNullable(pxContext)
									: namedPropertyType;
			}
			else
			{
				typeToCompare = property.Type;
			}

			if (fieldAttributeInfo.FieldType.Equals(typeToCompare))
				return null;

			return CreateDiagnosticsAsync(property, fieldAttribute, symbolContext, registerCodeFix: true);
		}

		private static async Task CreateDiagnosticsAsync(IPropertySymbol property, AttributeData fieldAttribute, SymbolAnalysisContext symbolContext,
														 bool registerCodeFix)
		{
			var diagnosticProperties = ImmutableDictionary.Create<string, string>()
														  .Add(DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString());
			Location[] locations = await Task.WhenAll(GetPropertyTypeLocationAsync(property, symbolContext.CancellationToken),
													  GetAttributeLocationAsync(fieldAttribute, symbolContext.CancellationToken));

			Location propertyTypeLocation = locations[0];
			Location attributeLocation = locations[1];

			if (propertyTypeLocation != null)
			{
				symbolContext.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, propertyTypeLocation, attributeLocation.ToEnumerable(),
						diagnosticProperties));
			}

			if (attributeLocation != null)
			{
				symbolContext.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, attributeLocation, propertyTypeLocation.ToEnumerable(),
						diagnosticProperties));
			}
		}

		private static async Task<Location> GetAttributeLocationAsync(AttributeData attribute, CancellationToken cancellationToken)
		{
			SyntaxNode attributeSyntaxNode = await attribute.ApplicationSyntaxReference.GetSyntaxAsync(cancellationToken)
																					   .ConfigureAwait(false);
			return attributeSyntaxNode?.GetLocation();
		}

		private static async Task<Location> GetPropertyTypeLocationAsync(IPropertySymbol property, CancellationToken cancellationToken)
		{
			SyntaxNode propertySyntaxNode = await property.GetSyntaxAsync(cancellationToken)
														  .ConfigureAwait(false);

			return propertySyntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax
				? propertyDeclarationSyntax.Type.GetLocation()
				: property.Locations.FirstOrDefault();
		}
	}
}