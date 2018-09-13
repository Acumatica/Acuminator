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

			bool validSpecialTypes = await CheckForMultipleSpecialAttributesAsync(attributesWithInfos, symbolContext).ConfigureAwait(false);

			if (!validSpecialTypes || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			if (attributesWithInfos.Count > 1)
			{


				var locationTasks = attributesWithInfos.Select(info => GetAttributeLocationAsync(info.Attribute, symbolContext.CancellationToken));
				Location[] attributeLocations = await Task.WhenAll(locationTasks)
														  .ConfigureAwait(false);

				foreach (Location attrLocation in attributeLocations)
				{
					if (attrLocation == null)
						continue;

					symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1023_MultipleTypeAttributesOnProperty, attrLocation));
				}
			}
			else
			{
				var (fieldAttribute, fieldAttrInfo) = attributesWithInfos[0];
				await CheckAttributeAndPropertyTypesForCompatibilityAsync(property, fieldAttribute, fieldAttrInfo, pxContext, symbolContext);
			}
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

		private static async Task<bool> CheckForMultipleSpecialAttributesAsync(
															List<(AttributeData Attribute, List<FieldTypeAttributeInfo> Infos)> attributesWithInfos,
															SymbolAnalysisContext symbolContext)
		{
            List<AttributeData> specialAttributesDeclaredOnDacProperty = new List<AttributeData>(2);
            List<AttributeData> specialAttributesWithConflictingAggregatorDeclarations = new List<AttributeData>(2);

            foreach (var attrWithInfos in attributesWithInfos)
            {
                int countOfSpecialAttributeDeclarations = attrWithInfos.Infos.Count(info => info.IsSpecial);

                if (countOfSpecialAttributeDeclarations > 0)
                {
                    specialAttributesDeclaredOnDacProperty.Add(attrWithInfos.Attribute);
                }

                if (countOfSpecialAttributeDeclarations > 1)
                {
                    specialAttributesWithConflictingAggregatorDeclarations.Add(attrWithInfos.Attribute);
                }
            }
             
			if (specialAttributesDeclaredOnDacProperty.Count == 0)
				return true;
            
            await RegisterDiagnosticForAttributesAsync(symbolContext, attributesWithMultipleSpecialAttribute, 
                                                       Descriptors.PX1023_MultipleSpecialTypeAttributesOnAggregators)
                                                 .ConfigureAwait(false);
            return false;
		}



		private static Task CheckAttributeAndPropertyTypesForCompatibilityAsync(IPropertySymbol property, AttributeData fieldAttribute,
																				FieldTypeAttributeInfo fieldAttributeInfo, PXContext pxContext,
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