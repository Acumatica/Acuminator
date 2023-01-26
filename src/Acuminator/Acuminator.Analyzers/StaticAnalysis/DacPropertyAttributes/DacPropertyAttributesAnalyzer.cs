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

			if (attributesCalcedOnDbSideDeclaredOnDacProperty.Count == 0 ||
				(attributesCalcedOnDbSideDeclaredOnDacProperty.Count == 1 && attributesCalcedOnDbSideWithConflictingAggregatorDeclarations.Count == 0))
			{
				return true;
			}

			if (attributesCalcedOnDbSideDeclaredOnDacProperty.Count > 1)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, attributesCalcedOnDbSideDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnProperty);
			}

			if (attributesCalcedOnDbSideWithConflictingAggregatorDeclarations.Count > 0)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, attributesCalcedOnDbSideDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleCalcedOnDbSideAttributesOnAggregators);
			}

			return false;

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeInfo>, List<AttributeInfo>) FilterAttributeInfosCalcedOnDbSide()
			{
				List<AttributeInfo> attributesCalcedOnDbSideOnDacProperty = new List<AttributeInfo>(2);
				List<AttributeInfo> attributesCalcedOnDbSideInvalidAggregatorDeclarations = new List<AttributeInfo>(2);

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
						attributesCalcedOnDbSideOnDacProperty.Add(attribute);

					if (counterOfCalcedOnDbSideAttributeInfos > 1)
						attributesCalcedOnDbSideInvalidAggregatorDeclarations.Add(attribute);
				}

				return (attributesCalcedOnDbSideOnDacProperty, attributesCalcedOnDbSideInvalidAggregatorDeclarations);
			}
		}

		private static void CheckForPXDBCalcedAndUnboundTypeAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext, IPropertySymbol propertySymbol,
																	   List<AttributeInfo> attributesWithFieldTypeMetadata)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			bool hasPXDBCalcedAttribute =
				attributesWithFieldTypeMetadata.Any(attrInfo => attrInfo.BoundnessType == DbBoundnessType.PXDBCalced);

			if (!hasPXDBCalcedAttribute)
				return;

			bool hasUnboundTypeAttribute =
				attributesWithFieldTypeMetadata.Any(attrInfo => attrInfo.BoundnessType == DbBoundnessType.Unbound);
				
			if (hasUnboundTypeAttribute || propertySymbol.GetSyntax(symbolContext.CancellationToken) is not PropertyDeclarationSyntax propertyNode)
				return;

			var diagnostic = Diagnostic.Create(Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute, propertyNode.Identifier.GetLocation());
			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private static void CheckForFieldTypeAttributes(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														List<AttributeInfo> attributesWithFieldTypeMetadata)
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
				AttributeInfo typeAttribute = typeAttributesDeclaredOnDacProperty[0];
				var attributeDataType = typeAttribute.AggregatedAttributeMetadata
													 .Where(atrMetadata => atrMetadata.IsFieldAttribute && atrMetadata.DataType != null)
													 .Select(atrMetadata => atrMetadata.DataType)
													 .Distinct()
													 .FirstOrDefault();
		
				CheckAttributeAndPropertyTypesForCompatibility(property, typeAttribute, attributeDataType, pxContext, symbolContext);
			}

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeInfo>, List<AttributeInfo>) FilterTypeAttributes()
			{
				var typeAttributesOnDacProperty = new List<AttributeInfo>(2);
				var typeAttributesWithInvalidAggregatorDeclarations = new List<AttributeInfo>();

				foreach (var attribute in attributesWithFieldTypeMetadata)
				{
					var dataTypeAttributes = attribute.AggregatedAttributeMetadata
													  .Where(atrMetadata => atrMetadata.IsFieldAttribute)
													  .ToList();

					if (dataTypeAttributes.Count == 0)
						continue;

					typeAttributesOnDacProperty.Add(attribute);

					if (dataTypeAttributes.Count == 1)
						continue;

					int countOfDeclaredDataTypes = dataTypeAttributes.Select(atrMetadata => atrMetadata.DataType)
																	 .Distinct()
																	 .Count();

					if (countOfDeclaredDataTypes > 1 || dataTypeAttributes.Any(atrMetadata => atrMetadata.DataType == null))
						typeAttributesWithInvalidAggregatorDeclarations.Add(attribute);
				}

				return (typeAttributesOnDacProperty, typeAttributesWithInvalidAggregatorDeclarations);
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