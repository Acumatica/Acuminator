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
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute,
				Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute
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

			bool validAttributesCalcedOnDbSide = CheckForMultipleAttributesCalcedOnDbSide(symbolContext, pxContext, property, 
																						  attributesWithFieldTypeMetadata);
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (!validAttributesCalcedOnDbSide)
				return;

			CheckForCalcedOnDbSideAndUnboundTypeAttributes(symbolContext, pxContext, property, attributesWithFieldTypeMetadata);
			CheckForFieldTypeAttributes(property, symbolContext, pxContext, attributesWithFieldTypeMetadata);
		}
	
		private static bool CheckForMultipleAttributesCalcedOnDbSide(SymbolAnalysisContext symbolContext, PXContext pxContext,
																	 DacPropertyInfo property, List<AttributeInfo> attributesWithFieldTypeMetadata)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			// Optimization - properties with the following DB boudnesses defintely don't have attributes calced on DB side
			if (property.EffectiveDbBoundness is DbBoundnessType.DbBound or DbBoundnessType.Unbound or DbBoundnessType.NotDefined)
				return true;

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

		private static void CheckForCalcedOnDbSideAndUnboundTypeAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
																		   DacPropertyInfo property, List<AttributeInfo> attributesWithFieldTypeMetadata)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			// Optimization - properties with the following DB boudnesses defintely don't have attributes calced on DB side
			if (property.EffectiveDbBoundness is DbBoundnessType.DbBound or DbBoundnessType.Unbound or DbBoundnessType.NotDefined)
				return;

			bool hasPXDBCalcedAttribute = false, hasPXDBScalarAttribute = false, hasUnboundTypeAttribute = false;

			foreach (var attrInfo in attributesWithFieldTypeMetadata)
			{
				switch (attrInfo.DbBoundness)
				{
					case DbBoundnessType.Unbound:
						hasUnboundTypeAttribute = true;
						continue;	
					case DbBoundnessType.PXDBScalar:
						hasPXDBScalarAttribute = true;
						continue;
					case DbBoundnessType.PXDBCalced:
						hasPXDBCalcedAttribute = true;
						continue;			
				}
			}

			if (hasUnboundTypeAttribute || (!hasPXDBCalcedAttribute && !hasPXDBScalarAttribute) ||
				property.Node.Identifier.GetLocation() is not Location location)
			{
				return;
			}

			if (hasPXDBCalcedAttribute)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute, location);
				symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}

			if (hasPXDBScalarAttribute)
			{
				var diagnostic = Diagnostic.Create(Descriptors.PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute, location);
				symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
			}
		}

		private static void CheckForFieldTypeAttributes(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														List<AttributeInfo> attributesWithFieldTypeMetadata)
		{
			if (property.EffectiveDbBoundness == DbBoundnessType.NotDefined)
				return;

			var (typeAttributesOnDacProperty, typeAttributesWithDifferentDataTypesOnAggregator, hasNonNullDataType) = 
				FilterTypeAttributes(attributesWithFieldTypeMetadata);

			if (typeAttributesOnDacProperty.IsNullOrEmpty() || !hasNonNullDataType)
				return;

			if (typeAttributesWithDifferentDataTypesOnAggregator?.Count > 0)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, typeAttributesWithDifferentDataTypesOnAggregator,
												Descriptors.PX1023_MultipleTypeAttributesOnAggregators);
			}

			if (typeAttributesOnDacProperty.Count > 1)					
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, typeAttributesOnDacProperty,
												Descriptors.PX1023_MultipleTypeAttributesOnProperty);
			} 
			else if (typeAttributesWithDifferentDataTypesOnAggregator?.Count is null or 0)
			{
				AttributeInfo dataTypeAttribute = typeAttributesOnDacProperty[0];
				var dataTypeFromAttribute = dataTypeAttribute.AggregatedAttributeMetadata
															 .Where(atrMetadata => atrMetadata.IsFieldAttribute && atrMetadata.DataType != null)
															 .Select(atrMetadata => atrMetadata.DataType)
															 .Distinct()
															 .FirstOrDefault();
		
				CheckAttributeAndPropertyTypesForCompatibility(property, dataTypeAttribute, dataTypeFromAttribute, pxContext, symbolContext);
			}			
		}

		private static (List<AttributeInfo>?, List<AttributeInfo>?, bool HasNonNullDataType) FilterTypeAttributes(
																								List<AttributeInfo> attributesWithFieldTypeMetadata)
		{
			List<AttributeInfo>? typeAttributesOnDacProperty = null;
			List<AttributeInfo>? typeAttributesWithDifferentDataTypesOnAggregator = null;
			bool hasNonNullDataType = false;

			foreach (var attribute in attributesWithFieldTypeMetadata)
			{
				var dataTypeAttributes = attribute.AggregatedAttributeMetadata
												  .Where(atrMetadata => atrMetadata.IsFieldAttribute)
												  .ToList();
				if (dataTypeAttributes.Count == 0)
					continue;

				typeAttributesOnDacProperty ??= new List<AttributeInfo>(capacity: 2);
				typeAttributesOnDacProperty.Add(attribute);

				if (dataTypeAttributes.Count == 1)
				{
					hasNonNullDataType = hasNonNullDataType || dataTypeAttributes[0].DataType != null;
					continue;
				}

				int countOfDeclaredNonNullDataTypes = dataTypeAttributes.Where(atrMetadata => atrMetadata.DataType != null)
																		.Select(atrMetadata => atrMetadata.DataType)
																		.Distinct()
																		.Count();
				hasNonNullDataType = hasNonNullDataType || countOfDeclaredNonNullDataTypes > 0;

				if (countOfDeclaredNonNullDataTypes > 1)
				{
					typeAttributesWithDifferentDataTypesOnAggregator ??= new List<AttributeInfo>(capacity: 2);
					typeAttributesWithDifferentDataTypesOnAggregator.Add(attribute);
				}
			}

			return (typeAttributesOnDacProperty, typeAttributesWithDifferentDataTypesOnAggregator, hasNonNullDataType);
		}

		private static void CheckAttributeAndPropertyTypesForCompatibility(DacPropertyInfo property, AttributeInfo dataTypeAttribute, 
																		   ITypeSymbol? dataTypeFromAttribute, PXContext pxContext, 
																		   SymbolAnalysisContext symbolContext)
		{
			if (dataTypeFromAttribute == null)             //PXDBFieldAttribute and PXEntityAttribute without data type case
			{
				ReportIncompatibleTypesDiagnostics(property, dataTypeAttribute, symbolContext, pxContext, registerCodeFix: false);
				return;
			}

			if (!dataTypeFromAttribute.Equals(property.EffectivePropertyType))
			{
				ReportIncompatibleTypesDiagnostics(property, dataTypeAttribute, symbolContext, pxContext, registerCodeFix: true);
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