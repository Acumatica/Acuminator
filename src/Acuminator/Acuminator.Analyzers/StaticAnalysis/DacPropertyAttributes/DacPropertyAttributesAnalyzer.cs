﻿#nullable enable

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
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty,
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators,
				Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExt)
		{
			FieldTypeAttributesRegister fieldAttributesRegister = new FieldTypeAttributesRegister(pxContext);

			foreach (DacPropertyInfo property in dacOrDacExt.AllDeclaredProperties)
			{
				CheckDacProperty(property, context, pxContext, fieldAttributesRegister);
			}	
		}

		private static void CheckDacProperty(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext,
											 FieldTypeAttributesRegister fieldAttributesRegister)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			ImmutableArray<AttributeInfo> attributes = property.Attributes;

			if (attributes.IsDefaultOrEmpty)
				return;

			var attributesWithInfos = GetFieldTypeAttributesInfos(attributes, fieldAttributesRegister, symbolContext.CancellationToken);

			if (attributesWithInfos.Count == 0)
				return;

			bool validAttributesCalcedOnDbSide = CheckForMultipleAttributesCalcedOnDbSide(symbolContext, pxContext, attributesWithInfos);
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (!validAttributesCalcedOnDbSide)
				return;

			CheckForPXDBCalcedAndUnboundTypeAttributes(symbolContext, pxContext, property.Symbol, attributesWithInfos);
			CheckForFieldTypeAttributes(property, symbolContext, pxContext, attributesWithInfos);
		}
	
		private static List<(AttributeInfo Attribute, IReadOnlyCollection<FieldTypeAttributeInfo> Infos)> GetFieldTypeAttributesInfos(
																													ImmutableArray<AttributeInfo> attributes,
																													FieldTypeAttributesRegister fieldAttributesRegister,
																													CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var fieldInfosList = new List<(AttributeInfo, IReadOnlyCollection<FieldTypeAttributeInfo>)>(capacity: attributes.Length);

			foreach (AttributeInfo attribute in attributes)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var attributeInfos = fieldAttributesRegister.GetDacFieldTypeAttributeInfos(attribute.AttributeData.AttributeClass);

				if (attributeInfos.Count > 0)
				{
					fieldInfosList.Add((attribute, attributeInfos));
				}
			}

			return fieldInfosList;
		}

		private static bool CheckForMultipleAttributesCalcedOnDbSide(SymbolAnalysisContext symbolContext, PXContext pxContext,
																List<(AttributeInfo Attribute, IReadOnlyCollection<FieldTypeAttributeInfo> Infos)> attributesWithInfos)
		{
			var (attributesCalcedOnDbSideDeclaredOnDacProperty, attributesCalcedOnDbSideWithConflictingAggregatorDeclarations) = 
				FilterAttributesCalcedOnDbSide();

			if (attributesCalcedOnDbSideDeclaredOnDacProperty.Count == 0 ||
				(attributesCalcedOnDbSideDeclaredOnDacProperty.Count == 1 && attributesCalcedOnDbSideWithConflictingAggregatorDeclarations.Count == 0))
			{
				return true;
			}

			if (attributesCalcedOnDbSideDeclaredOnDacProperty.Count > 1)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, attributesCalcedOnDbSideDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty);
			}

			if (attributesCalcedOnDbSideWithConflictingAggregatorDeclarations.Count > 0)
			{
				RegisterDiagnosticForAttributes(symbolContext, pxContext, attributesCalcedOnDbSideDeclaredOnDacProperty,
												Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators);
			}

			return false;

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeInfo>, List<AttributeInfo>) FilterAttributesCalcedOnDbSide()
			{
				List<AttributeInfo> attributesCalcedOnDbSideOnDacProperty = new List<AttributeInfo>(2);
				List<AttributeInfo> attributesCalcedOnDbSideInvalidAggregatorDeclarations = new List<AttributeInfo>(2);

				foreach (var (attribute, infos) in attributesWithInfos)
				{
					int countOfCalcedOnDbSideAttributeInfos = infos.Count(atrInfo => atrInfo.IsCalculatedOnDbSide);

					if (countOfCalcedOnDbSideAttributeInfos > 0)
					{
						attributesCalcedOnDbSideOnDacProperty.Add(attribute);
					}

					if (countOfCalcedOnDbSideAttributeInfos > 1)
					{
						attributesCalcedOnDbSideInvalidAggregatorDeclarations.Add(attribute);
					}
				}

				return (attributesCalcedOnDbSideOnDacProperty, attributesCalcedOnDbSideInvalidAggregatorDeclarations);
			}
		}

		private static void CheckForPXDBCalcedAndUnboundTypeAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext, IPropertySymbol propertySymbol,
															List<(AttributeInfo Attribute, IReadOnlyCollection<FieldTypeAttributeInfo> Infos)> attributesWithInfos)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var (pxDBCalcedAttribute, _) = attributesWithInfos.FirstOrDefault(attrWithInfos => 
																				attrWithInfos.Infos.Any(i => i.Kind == FieldTypeAttributeKind.PXDBCalcedAttribute));
			if (pxDBCalcedAttribute == null)
				return;

			bool hasUnboundTypeAttribute = attributesWithInfos.Any(attrWithInfos => !ReferenceEquals(attrWithInfos.Attribute, pxDBCalcedAttribute) &&
																					attrWithInfos.Attribute.BoundType == DbBoundnessType.Unbound);
			if (hasUnboundTypeAttribute)
				return;

			if (!(propertySymbol.GetSyntax(symbolContext.CancellationToken) is PropertyDeclarationSyntax propertyNode))
				return;

			var diagnostic = Diagnostic.Create(Descriptors.PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute, propertyNode.Identifier.GetLocation());
			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private static void CheckForFieldTypeAttributes(DacPropertyInfo property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														List<(AttributeInfo Attribute, IReadOnlyCollection<FieldTypeAttributeInfo> Infos)> attributesWithInfos)
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
			(List<AttributeInfo>, List<AttributeInfo>) FilterTypeAttributes()
			{
				List<AttributeInfo> typeAttributesOnDacProperty = new List<AttributeInfo>(2);
				List<AttributeInfo> typeAttributesWithInvalidAggregatorDeclarations = new List<AttributeInfo>(2);

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

		private static void CheckAttributeAndPropertyTypesForCompatibility(DacPropertyInfo property, AttributeInfo fieldAttribute, ITypeSymbol? fieldType,
																		   PXContext pxContext, SymbolAnalysisContext symbolContext)
		{
			if (fieldType == null)                           //PXDBFieldAttribute case
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

			if (!fieldType.Equals(typeToCompare))
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