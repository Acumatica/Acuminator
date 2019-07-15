using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes
{
	public class DacPropertyAttributesAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty,
				Descriptors.PX1023_MultipleTypeAttributesOnProperty,
				Descriptors.PX1023_MultipleTypeAttributesOnAggregators,
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty,
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators,
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExt)
		{
			FieldTypeAttributesRegister fieldAttributesRegister = new FieldTypeAttributesRegister(pxContext);
			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			Parallel.ForEach(dacOrDacExt.DeclaredProperties, parallelOptions,
							 property => CheckDacProperty(property, context, pxContext, fieldAttributesRegister));
		}

		private static void CheckDacProperty(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext,
											 FieldTypeAttributesRegister fieldAttributesRegister)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			ImmutableArray<AttributeData> attributes = property.Symbol.GetAttributes();

			if (attributes.Length == 0)
				return;

			var attributesWithInfos = GetFieldTypeAttributesInfos(pxContext, attributes, fieldAttributesRegister, symbolContext.CancellationToken);

			if (attributesWithInfos.Count == 0)
				return;

			bool validSpecialTypes = CheckForMultipleSpecialAttributes(symbolContext, pxContext, attributesWithInfos);
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (!validSpecialTypes)
				return;

			CheckForPXDBCalcedAndUnboundTypeAttributes(symbolContext, pxContext, fieldAttributesRegister, property.Symbol, attributesWithInfos);
			CheckForFieldTypeAttributes(property.Symbol, symbolContext, pxContext, attributesWithInfos);
		}

		private static void CheckForPXDBCalcedAndUnboundTypeAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
														FieldTypeAttributesRegister fieldAttributesRegister, IPropertySymbol propertySymbol,
														List<(AttributeData Attribute, List<FieldTypeAttributeInfo> Infos)> attributesWithInfos)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var hasUnboundTypeAttribute = false;
			var hasPXDBCalcedAttribute = false;

			foreach (var (attribute, infos) in attributesWithInfos)
			{
				hasPXDBCalcedAttribute |= infos.Any(i => i.Kind == FieldTypeAttributeKind.PXDBCalcedAttribute);
				hasUnboundTypeAttribute |= infos.Any(i => i.Kind == FieldTypeAttributeKind.UnboundTypeAttribute);
			}

			if (!hasPXDBCalcedAttribute || hasUnboundTypeAttribute)
				return;

			if (!(propertySymbol.GetSyntax(symbolContext.CancellationToken) is PropertyDeclarationSyntax propertyNode))
				return;
	
			var diagnostic = Diagnostic.Create(Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute, propertyNode.Identifier.GetLocation());
			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);		
		}

		private static List<(AttributeData Attribute, List<FieldTypeAttributeInfo> Infos)> GetFieldTypeAttributesInfos(PXContext pxContext,
																									   ImmutableArray<AttributeData> attributes,
																									   FieldTypeAttributesRegister fieldAttributesRegister,
																									   CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var fieldInfosList = new List<(AttributeData, List<FieldTypeAttributeInfo>)>(capacity: attributes.Length);

			foreach (AttributeData attribute in attributes)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var attributeInfos = fieldAttributesRegister.GetFieldTypeAttributeInfos(attribute.AttributeClass).ToList();

				if (attributeInfos.Count > 0)
				{
					fieldInfosList.Add((attribute, attributeInfos));
				}
			}

			return fieldInfosList;
		}

		private static bool CheckForMultipleSpecialAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
															  List<(AttributeData Attribute, List<FieldTypeAttributeInfo> Infos)> attributesWithInfos)
		{
			var (specialAttributesDeclaredOnDacProperty, specialAttributesWithConflictingAggregatorDeclarations) = FilterSpecialAttributes();

			if (specialAttributesDeclaredOnDacProperty.Count == 0 ||
				(specialAttributesDeclaredOnDacProperty.Count == 1 && specialAttributesWithConflictingAggregatorDeclarations.Count == 0))
			{
				return true;
			}

			if (specialAttributesDeclaredOnDacProperty.Count > 1)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, specialAttributesDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty);
			}

			if (specialAttributesWithConflictingAggregatorDeclarations.Count > 0)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, specialAttributesDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators);
			}

			return false;

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeData>, List<AttributeData>) FilterSpecialAttributes()
			{
				List<AttributeData> specialAttributesOnDacProperty = new List<AttributeData>(2);
				List<AttributeData> specialAttributesInvalidAggregatorDeclarations = new List<AttributeData>(2);

				foreach (var (attribute, infos) in attributesWithInfos)
				{
					int countOfSpecialAttributeInfos = infos.Count(atrInfo => atrInfo.IsSpecial);

					if (countOfSpecialAttributeInfos > 0)
					{
						specialAttributesOnDacProperty.Add(attribute);
					}

					if (countOfSpecialAttributeInfos > 1)
					{
						specialAttributesInvalidAggregatorDeclarations.Add(attribute);
					}
				}

				return (specialAttributesOnDacProperty, specialAttributesInvalidAggregatorDeclarations);
			}
		}

		private static void CheckForFieldTypeAttributes(IPropertySymbol property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														List<(AttributeData Attribute, List<FieldTypeAttributeInfo> Infos)> attributesWithInfos)
		{
			var (typeAttributesDeclaredOnDacProperty, typeAttributesWithConflictingAggregatorDeclarations) = FilterTypeAttributes();

			if (typeAttributesDeclaredOnDacProperty.Count == 0)
				return;

			if (typeAttributesWithConflictingAggregatorDeclarations.Count > 0)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, typeAttributesWithConflictingAggregatorDeclarations,
												Descriptors.PX1023_MultipleTypeAttributesOnAggregators);
			}

			if (typeAttributesDeclaredOnDacProperty.Count > 1)					
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, typeAttributesDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleTypeAttributesOnProperty);
			} 
			else if (typeAttributesWithConflictingAggregatorDeclarations.Count == 0)
			{
				var typeAttribute = typeAttributesDeclaredOnDacProperty[0];
				var fieldType = attributesWithInfos.First(attrWithInfo => attrWithInfo.Attribute.Equals(typeAttribute))
												   .Infos
												   .Where(info => info.IsFieldAttribute)
												   .Select(info => info.FieldType)
												   .Distinct()
												   .SingleOrDefault();
		
				CheckAttributeAndPropertyTypesForCompatibility(property, typeAttribute, fieldType, pxContext, symbolContext);
			}

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeData>, List<AttributeData>) FilterTypeAttributes()
			{
				List<AttributeData> typeAttributesOnDacProperty = new List<AttributeData>(2);
				List<AttributeData> typeAttributesWithInvalidAggregatorDeclarations = new List<AttributeData>(2);

				foreach (var (attribute, infos) in attributesWithInfos)
				{
					int countOfTypeAttributeInfos = infos.Count(info => info.IsFieldAttribute);

					if (countOfTypeAttributeInfos == 0)
						continue;

					typeAttributesOnDacProperty.Add(attribute);

					if (countOfTypeAttributeInfos == 1)
						continue;

					int countOfDeclaredFieldTypes = infos.Select(info => info.FieldType)
														 .Distinct()
														 .Count();
					if (countOfDeclaredFieldTypes > 1)
					{
						typeAttributesWithInvalidAggregatorDeclarations.Add(attribute);
					}
				}

				return (typeAttributesOnDacProperty, typeAttributesWithInvalidAggregatorDeclarations);
			}
		}

		private static void CheckAttributeAndPropertyTypesForCompatibility(IPropertySymbol property, AttributeData fieldAttribute, ITypeSymbol fieldType,
																		   PXContext pxContext, SymbolAnalysisContext symbolContext)
		{
			if (fieldType == null)                           //PXDBFieldAttribute case
			{
				ReportIncompatibleTypesDiagnostics(property, fieldAttribute, symbolContext, pxContext, registerCodeFix: false);
				return;
			}

			ITypeSymbol typeToCompare;

			if (property.Type.IsValueType)
			{
				if (!(property.Type is INamedTypeSymbol namedPropertyType))
					return;

				typeToCompare = namedPropertyType.IsNullable(pxContext)
									? namedPropertyType.GetUnderlyingTypeFromNullable(pxContext)
									: namedPropertyType;
			}
			else
			{
				typeToCompare = property.Type;
			}

			if (!fieldType.Equals(typeToCompare))
			{
				ReportIncompatibleTypesDiagnostics(property, fieldAttribute, symbolContext, pxContext, registerCodeFix: true);
			}
		}

		private static void ReportIncompatibleTypesDiagnostics(IPropertySymbol property, AttributeData fieldAttribute,
															   SymbolAnalysisContext symbolContext, PXContext pxContext, bool registerCodeFix)
		{
			var diagnosticProperties = ImmutableDictionary.Create<string, string>()
														  .Add(DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString());
			Location propertyTypeLocation = GetPropertyTypeLocation(property, symbolContext.CancellationToken);
			Location attributeLocation = GetAttributeLocation(fieldAttribute, symbolContext.CancellationToken);

			if (propertyTypeLocation != null)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, propertyTypeLocation, attributeLocation.ToEnumerable(),
									  diagnosticProperties),
					pxContext.CodeAnalysisSettings);
			}

			if (attributeLocation != null)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, attributeLocation, propertyTypeLocation.ToEnumerable(),
									  diagnosticProperties),
					pxContext.CodeAnalysisSettings);
			}
		}

		private static void RegisterDiagnosticForAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
															IEnumerable<AttributeData> attributesToReport, DiagnosticDescriptor diagnosticDescriptor)
		{
			Location[] attributeLocations = attributesToReport.Select(a => GetAttributeLocation(a, symbolContext.CancellationToken))
															  .Where(location => location != null)
															  .ToArray();

			foreach (Location location in attributeLocations)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(diagnosticDescriptor, location), 
					pxContext.CodeAnalysisSettings);
			}
		}

		private static Location GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken) =>
			attribute.ApplicationSyntaxReference.GetSyntax(cancellationToken)?.GetLocation();

		private static Location GetPropertyTypeLocation(IPropertySymbol property, CancellationToken cancellationToken)
		{
			SyntaxNode propertySyntaxNode = property.GetSyntax(cancellationToken);

			return propertySyntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax
				? propertyDeclarationSyntax.Type.GetLocation()
				: property.Locations.FirstOrDefault();
		}
	}
}