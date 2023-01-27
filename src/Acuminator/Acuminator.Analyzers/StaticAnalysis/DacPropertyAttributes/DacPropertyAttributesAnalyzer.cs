#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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
				Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnProperty,
				Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnAggregators,
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExt)
		{
			foreach (DacPropertyInfo property in dacOrDacExt.AllDeclaredProperties)
			{
				CheckDacProperty(property, context, pxContext);
			}	
		}

		private static void CheckDacProperty(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			ImmutableArray<AttributeInfo> attributes = property.Attributes;

			if (attributes.IsDefaultOrEmpty)
				return;

			var attributesWithFieldTypeMetadata = attributes.Where(aInfo => !aInfo.AggregatedAttributeMetadata.IsDefaultOrEmpty)
															.ToList(capacity: attributes.Length);
			if (attributesWithFieldTypeMetadata.Count == 0)
				return;

			bool validAttributesCalcedOnDbSide = CheckForMultipleAttributesCalcedOnDbSide(symbolContext, pxContext, attributesWithFieldTypeMetadata);
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (!validAttributesCalcedOnDbSide)
				return;

			CheckForPXDBCalcedAndUnboundTypeAttributes(symbolContext, pxContext, property.Symbol, attributesWithFieldTypeMetadata);
			CheckForFieldTypeAttributes(property, symbolContext, pxContext, attributesWithFieldTypeMetadata);
		}
	
		private static bool CheckForMultipleAttributesCalcedOnDbSide(SymbolAnalysisContext symbolContext, PXContext pxContext, 
																	 List<AttributeInfo> attributesWithFieldTypeMetadata)
		{
			var (attributesCalcedOnDbSideDeclaredOnDacProperty, attributesCalcedOnDbSideWithConflictingAggregatorDeclarations) =
				FilterAttributeInfosCalcedOnDbSide();

			if (attributesCalcedOnDbSideDeclaredOnDacProperty.IsNullOrEmpty() ||
				(attributesCalcedOnDbSideDeclaredOnDacProperty.Count == 1 && attributesCalcedOnDbSideWithConflictingAggregatorDeclarations.IsNullOrEmpty()))
			{
				return true;
			}

			if (attributesCalcedOnDbSideDeclaredOnDacProperty.Count > 1)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, attributesCalcedOnDbSideDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnProperty);
			}

			if (attributesCalcedOnDbSideWithConflictingAggregatorDeclarations?.Count > 0)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, attributesCalcedOnDbSideDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnAggregators);
			}

			return false;

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeInfo>?, List<AttributeInfo>?) FilterAttributeInfosCalcedOnDbSide()
			{
				List<AttributeInfo>? attributesCalcedOnDbSideOnDacProperty = null;
				List<AttributeInfo>? attributesCalcedOnDbSideInvalidAggregatorDeclarations = null;

				foreach (var attribute in attributesWithFieldTypeMetadata)
				{
					int counterOfCalcedOnDbSideAttributeInfos = 0;
					var dbCalcedFieldTypeAttributes = attribute.AggregatedAttributeMetadata.Where(atrMetadata => atrMetadata.IsCalculatedOnDbSide);

					foreach (var dbCalcedAttributeInfo in dbCalcedFieldTypeAttributes)
					{
						counterOfCalcedOnDbSideAttributeInfos++;

						if (counterOfCalcedOnDbSideAttributeInfos > 1)
							break;
					}

					if (counterOfCalcedOnDbSideAttributeInfos > 0)
					{
						attributesCalcedOnDbSideOnDacProperty ??= new List<AttributeInfo>(capacity: 2);
						attributesCalcedOnDbSideOnDacProperty.Add(attribute);
					}

					if (counterOfCalcedOnDbSideAttributeInfos > 1)
					{
						attributesCalcedOnDbSideInvalidAggregatorDeclarations ??= new List<AttributeInfo>(capacity: 2);
						attributesCalcedOnDbSideInvalidAggregatorDeclarations.Add(attribute);
					}
				}

				return (attributesCalcedOnDbSideOnDacProperty, attributesCalcedOnDbSideInvalidAggregatorDeclarations);
			}
		}

		private static void CheckForPXDBCalcedAndUnboundTypeAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext, IPropertySymbol propertySymbol,
																	   List<AttributeInfo> attributesWithFieldTypeMetadata)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			bool hasPXDBCalcedAttribute =
				attributesWithFieldTypeMetadata.Any(attrInfo => attrInfo.DbBoundness == DbBoundnessType.PXDBCalced);

			if (!hasPXDBCalcedAttribute)
				return;

			bool hasUnboundTypeAttribute =
				attributesWithFieldTypeMetadata.Any(attrInfo => attrInfo.DbBoundness == DbBoundnessType.Unbound);
				
			if (hasUnboundTypeAttribute || propertySymbol.GetSyntax(symbolContext.CancellationToken) is not PropertyDeclarationSyntax propertyNode)
				return;

			var diagnostic = Diagnostic.Create(Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute, propertyNode.Identifier.GetLocation());
			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private static void CheckForFieldTypeAttributes(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														List<AttributeInfo> attributesWithFieldTypeMetadata)
		{
			var (typeAttributesOnDacPropertyInfos, typeAttributesWithDifferentDataTypesOnAggregatorInfos, typeAttributesWithNoDataTypeInfos) = FilterTypeAttributes();

			if (typeAttributesOnDacPropertyInfos.IsNullOrEmpty())
				return;

			if (typeAttributesWithDifferentDataTypesOnAggregatorInfos?.Count > 0)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, typeAttributesWithDifferentDataTypesOnAggregatorInfos,
												Descriptors.PX1023_MultipleTypeAttributesOnAggregators);
			}

			if (typeAttributesOnDacPropertyInfos.Count > 1)					
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, typeAttributesOnDacPropertyInfos,
												Descriptors.PX1023_MultipleTypeAttributesOnProperty);
			} 
			else if (typeAttributesWithDifferentDataTypesOnAggregatorInfos?.Count is null or 0)
			{
				AttributeInfo typeAttribute = typeAttributesOnDacPropertyInfos[0];
				var attributeDataType = typeAttribute.AggregatedAttributeMetadata
													 .Where(atrMetadata => atrMetadata.IsFieldAttribute && atrMetadata.DataType != null)
													 .Select(atrMetadata => atrMetadata.DataType)
													 .Distinct()
													 .FirstOrDefault();
		
				CheckAttributeAndPropertyTypesForCompatibility(property, typeAttribute, attributeDataType, pxContext, symbolContext);
			}

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeInfo>?, List<AttributeInfo>?, List<AttributeInfo>?) FilterTypeAttributes()
			{
				List<AttributeInfo>? typeAttributesOnDacProperty = null;
				List<AttributeInfo>? typeAttributesWithDifferentDataTypesOnAggregator = null;
				List<AttributeInfo>? typeAttributesWithNoDataType = null;

				foreach (var attribute in attributesWithFieldTypeMetadata)
				{
					var dataTypeAttributes = attribute.AggregatedAttributeMetadata
													  .Where(atrMetadata => atrMetadata.IsFieldAttribute)
													  .ToList();
					if (dataTypeAttributes.Count == 0)
						continue;

					typeAttributesOnDacProperty ??= new List<AttributeInfo>(capacity: 2); 
					typeAttributesOnDacProperty.Add(attribute);

					bool allDataTypesAreNull = dataTypeAttributes.All(atrMetadata => atrMetadata.DataType == null);

					if (allDataTypesAreNull)
					{
						typeAttributesWithNoDataType ??= new List<AttributeInfo>(capacity: 2);
						typeAttributesWithNoDataType.Add(attribute);
						continue;
					}	

					if (dataTypeAttributes.Count == 1)
						continue;

					int countOfDeclaredNonNullDataTypes = dataTypeAttributes.Where(atrMetadata => atrMetadata.DataType != null)
																			.Select(atrMetadata => atrMetadata.DataType)
																			.Distinct()
																			.Count();
					if (countOfDeclaredNonNullDataTypes > 1)
					{
						typeAttributesWithDifferentDataTypesOnAggregator ??= new List<AttributeInfo>(capacity: 2);
						typeAttributesWithDifferentDataTypesOnAggregator.Add(attribute);
					}
				}

				return (typeAttributesOnDacProperty, typeAttributesWithDifferentDataTypesOnAggregator, typeAttributesWithNoDataType);
			}
		}

		private static void CheckAttributeAndPropertyTypesForCompatibility(DacPropertyInfo property, AttributeInfo fieldAttribute, ITypeSymbol? attributeDataType,
																		   PXContext pxContext, SymbolAnalysisContext symbolContext)
		{
			if (attributeDataType == null)                           //PXDBFieldAttribute case
			{
				ReportIncompatibleTypesDiagnostics(property, fieldAttribute, symbolContext, pxContext, registerCodeFix: false);
				return;
			}

			ITypeSymbol? typeToCompare;

			if (property.PropertyType.IsValueType)
			{
				typeToCompare = property.PropertyType.IsNullable(pxContext)
					? property.PropertyType.GetUnderlyingTypeFromNullable(pxContext)
					: property.PropertyType;
			}
			else
			{
				typeToCompare = property.PropertyType;
			}

			if (!attributeDataType.Equals(typeToCompare))
			{
				ReportIncompatibleTypesDiagnostics(property, fieldAttribute, symbolContext, pxContext, registerCodeFix: true);
			}
		}

		private static void ReportIncompatibleTypesDiagnostics(DacPropertyInfo property, AttributeInfo fieldAttribute,
															   SymbolAnalysisContext symbolContext, PXContext pxContext, bool registerCodeFix)
		{
			var diagnosticProperties = ImmutableDictionary.Create<string, string>()
														  .Add(DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString());
			Location? propertyTypeLocation = property.Node.Type.GetLocation();
			Location? attributeLocation = fieldAttribute.AttributeData.GetLocation(symbolContext.CancellationToken);

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
															IEnumerable<AttributeInfo> attributesToReport, DiagnosticDescriptor diagnosticDescriptor)
		{
			Location[] attributeLocations = attributesToReport.Select(a => a.AttributeData.GetLocation(symbolContext.CancellationToken))
															  .Where(location => location != null)
															  .ToArray()!;
			foreach (Location location in attributeLocations)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(diagnosticDescriptor, location), 
					pxContext.CodeAnalysisSettings);
			}
		}
	}
}