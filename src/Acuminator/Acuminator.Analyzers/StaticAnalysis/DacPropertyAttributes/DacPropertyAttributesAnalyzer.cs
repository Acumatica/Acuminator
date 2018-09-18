using System;
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
				Descriptors.PX1023_MultipleTypeAttributesOnProperty,
				Descriptors.PX1023_MultipleTypeAttributesOnAggregators,
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty,
				Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(symbolContext => AnalyzePropertyAsync(symbolContext, pxContext), SymbolKind.NamedType);
		}

		private static Task AnalyzePropertyAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (!(symbolContext.Symbol is INamedTypeSymbol dacOrDacExt) || !dacOrDacExt.IsDacOrExtension(pxContext))
				return Task.FromResult(false);

			FieldTypeAttributesRegister fieldAttributesRegister = new FieldTypeAttributesRegister(pxContext);
			Task[] allTasks = dacOrDacExt.GetMembers()
										 .OfType<IPropertySymbol>()
										 .Select(property => CheckDacPropertyAsync(property, symbolContext, pxContext, fieldAttributesRegister))
										 .ToArray();

			return Task.WhenAll(allTasks);
		}

		private static async Task CheckDacPropertyAsync(IPropertySymbol property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														FieldTypeAttributesRegister fieldAttributesRegister)
		{
			ImmutableArray<AttributeData> attributes = property.GetAttributes();

			if (attributes.Length == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var attributesWithInfos = GetFieldTypeAttributesInfos(pxContext, attributes, fieldAttributesRegister, symbolContext.CancellationToken);

			if (symbolContext.CancellationToken.IsCancellationRequested || attributesWithInfos.Count == 0)
				return;

			bool validSpecialTypes = await CheckForMultipleSpecialAttributesAsync(symbolContext, attributesWithInfos).ConfigureAwait(false);

			if (!validSpecialTypes || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			await CheckForFieldTypeAttributesAsync(property, symbolContext, pxContext, attributesWithInfos)
					.ConfigureAwait(false);
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

		private static async Task<bool> CheckForMultipleSpecialAttributesAsync(SymbolAnalysisContext symbolContext,
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
				await RegisterDiagnosticForAttributesAsync(symbolContext, specialAttributesDeclaredOnDacProperty,
														   Descriptors.PX1023_MultipleSpecialTypeAttributesOnProperty)
													  .ConfigureAwait(false);
			}

			if (specialAttributesWithConflictingAggregatorDeclarations.Count > 0)
			{
				await RegisterDiagnosticForAttributesAsync(symbolContext, specialAttributesDeclaredOnDacProperty,
														   Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators)
													  .ConfigureAwait(false);
			}

			return false;

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeData>, List<AttributeData>) FilterSpecialAttributes()
			{
				List<AttributeData> specialAttributesOnDacProperty = new List<AttributeData>(2);
				List<AttributeData> specialAttributesInvalidAggregatorDeclarations = new List<AttributeData>(2);

				foreach (var attrWithInfos in attributesWithInfos)
				{
					int countOfSpecialAttributeInfos = attrWithInfos.Infos.Count(atrInfo => atrInfo.IsSpecial);

					if (countOfSpecialAttributeInfos > 0)
					{
						specialAttributesOnDacProperty.Add(attrWithInfos.Attribute);
					}

					if (countOfSpecialAttributeInfos > 1)
					{
						specialAttributesInvalidAggregatorDeclarations.Add(attrWithInfos.Attribute);
					}
				}

				return (specialAttributesOnDacProperty, specialAttributesInvalidAggregatorDeclarations);
			}
		}

		private static async Task CheckForFieldTypeAttributesAsync(IPropertySymbol property, SymbolAnalysisContext symbolContext, PXContext pxContext,
														List<(AttributeData Attribute, List<FieldTypeAttributeInfo> Infos)> attributesWithInfos)
		{
			var (typeAttributesDeclaredOnDacProperty, typeAttributesWithConflictingAggregatorDeclarations) = FilterTypeAttributes();

			if (typeAttributesDeclaredOnDacProperty.Count == 0)
				return;

			if (typeAttributesWithConflictingAggregatorDeclarations.Count > 0)
			{
				await RegisterDiagnosticForAttributesAsync(symbolContext, typeAttributesWithConflictingAggregatorDeclarations,
														   Descriptors.PX1023_MultipleTypeAttributesOnAggregators)
													  .ConfigureAwait(false);
			}

			if (typeAttributesDeclaredOnDacProperty.Count > 1)					
			{
				await RegisterDiagnosticForAttributesAsync(symbolContext, typeAttributesDeclaredOnDacProperty,
														   Descriptors.PX1023_MultipleTypeAttributesOnProperty)
													  .ConfigureAwait(false);
			} 
			else if (typeAttributesWithConflictingAggregatorDeclarations.Count == 0)
			{
				var typeAttribute = typeAttributesDeclaredOnDacProperty[0];
				var attributeInfo = attributesWithInfos.First(attrWithInfo => attrWithInfo.Attribute.Equals(typeAttribute))
													   .Infos
													   .Single(info => info.IsFieldAttribute);
		
				await CheckAttributeAndPropertyTypesForCompatibilityAsync(property, typeAttribute, attributeInfo, pxContext, symbolContext);
			}

			//-----------------------------------------------Local Functions---------------------------------------
			(List<AttributeData>, List<AttributeData>) FilterTypeAttributes()
			{
				List<AttributeData> typeAttributesOnDacProperty = new List<AttributeData>(2);
				List<AttributeData> typeAttributesWithInvalidAggregatorDeclarations = new List<AttributeData>(2);

				foreach (var attrWithInfos in attributesWithInfos)
				{
					int countOfTypeAttributeInfos = attrWithInfos.Infos.Count(info => info.IsFieldAttribute);

					if (countOfTypeAttributeInfos == 0)
						continue;

					typeAttributesOnDacProperty.Add(attrWithInfos.Attribute);

					if (countOfTypeAttributeInfos == 1)
						continue;

					int countOfDeclaredFieldTypes = attrWithInfos.Infos.Select(info => info.FieldType)
																	   .Distinct()
																	   .Count();
					if (countOfDeclaredFieldTypes > 1)
					{
						typeAttributesWithInvalidAggregatorDeclarations.Add(attrWithInfos.Attribute);
					}
				}

				return (typeAttributesOnDacProperty, typeAttributesWithInvalidAggregatorDeclarations);
			}
		}

		private static Task CheckAttributeAndPropertyTypesForCompatibilityAsync(IPropertySymbol property, AttributeData fieldAttribute,
																				FieldTypeAttributeInfo fieldAttributeInfo, PXContext pxContext,
																				SymbolAnalysisContext symbolContext)
		{
			if (fieldAttributeInfo.FieldType == null)                                                               //PXDBFieldAttribute case
			{
				return ReportIncompatibleTypesDiagnosticsAsync(property, fieldAttribute, symbolContext, registerCodeFix: false);
			}

			ITypeSymbol typeToCompare;

			if (property.Type.IsValueType)
			{
				if (!(property.Type is INamedTypeSymbol namedPropertyType))
				{
					return Task.FromResult(false);
				}

				typeToCompare = namedPropertyType.IsNullable(pxContext)
									? namedPropertyType.GetUnderlyingTypeFromNullable(pxContext)
									: namedPropertyType;
			}
			else
			{
				typeToCompare = property.Type;
			}

			if (fieldAttributeInfo.FieldType.Equals(typeToCompare))
			{
				return Task.FromResult(false);
			}

			return ReportIncompatibleTypesDiagnosticsAsync(property, fieldAttribute, symbolContext, registerCodeFix: true);
		}

		private static async Task ReportIncompatibleTypesDiagnosticsAsync(IPropertySymbol property, AttributeData fieldAttribute,
																		  SymbolAnalysisContext symbolContext, bool registerCodeFix)
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

		private static async Task RegisterDiagnosticForAttributesAsync(SymbolAnalysisContext symbolContext,
																	   IEnumerable<AttributeData> attributesToReport,
																	   DiagnosticDescriptor diagnosticDescriptor)
		{
			var locationTasks = attributesToReport.Select(a => GetAttributeLocationAsync(a, symbolContext.CancellationToken)).ToArray();
			Location[] attributeLocations = await Task.WhenAll(locationTasks).ConfigureAwait(false);

			foreach (Location attrLocation in attributeLocations.Where(location => location != null))
			{
				symbolContext.ReportDiagnostic(
					Diagnostic.Create(diagnosticDescriptor, attrLocation));
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