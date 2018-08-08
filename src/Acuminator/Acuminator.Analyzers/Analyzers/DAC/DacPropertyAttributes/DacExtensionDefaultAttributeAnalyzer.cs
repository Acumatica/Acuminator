using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using System.Collections.Immutable;
using PX.Data;

namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    class DacExtensionDefaultAttributeAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1030_DefaultAttibuteToExisitingRecords
            );
#pragma warning disable CS4014
        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSymbolAction(symbolContext =>
                AnalyzePropertyAsync(symbolContext, pxContext), SymbolKind.Property);
        }
#pragma warning restore CS4014
        private static async Task AnalyzePropertyAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
        {
            IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;

            if (!CheckProperty(property, pxContext))
                return;

            ImmutableArray<AttributeData> attributes = property.GetAttributes();

            if (attributes.Length == 0 || symbolContext.CancellationToken.IsCancellationRequested)
                return;

			var attributesWithInfo = DacPropertyAttributesAnalyzer.GetFieldAttributesInfos(pxContext, attributes, symbolContext.CancellationToken);


			if (symbolContext.CancellationToken.IsCancellationRequested || attributesWithInfo.IsNullOrEmpty())
				return;

			if (attributesWithInfo.Count > 0 && attributesWithInfo[0].Info.IsBoundField) //field is DBBound
			{
				foreach (var attribute in attributes)
				{
					var typesHierarchy = attribute.AttributeClass.GetBaseTypesAndThis();
					if (typesHierarchy.Contains(symbolContext.Compilation.GetTypeByMetadataName(typeof(PX.Data.PXDefaultAttribute).FullName)))
					{
						Location[] locations = await Task.WhenAll(DacPropertyAttributesAnalyzer.GetAttributeLocationAsync(attribute, symbolContext.CancellationToken));
						Location attributeLocation = locations[0];

						if (attributeLocation != null)
						{
							foreach (var argument in attribute.NamedArguments)
							{
								if (argument.Key.Contains("PersistingCheck") && (int)argument.Value.Value == (int)PX.Data.PXPersistingCheck.Nothing)
									return;
							}
							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.PX1030_DefaultAttibuteToExisitingRecords, attributeLocation));

						}
					}
					// var allAttr = typesHierarchy.SelectMany(type => type.GetAttributes);
				}
			}
			else
			{
				foreach (var attribute in attributes)
				{
					var typesHierarchy = attribute.AttributeClass.GetBaseTypesAndThis();
					if (typesHierarchy.Contains(symbolContext.Compilation.GetTypeByMetadataName(typeof(PXDefaultAttribute).FullName)) &&
						!(typesHierarchy.Contains(symbolContext.Compilation.GetTypeByMetadataName(typeof(PXUnboundDefaultAttribute).FullName))))
					{
						Location[] locations = await Task.WhenAll(DacPropertyAttributesAnalyzer.GetAttributeLocationAsync(attribute, symbolContext.CancellationToken));
						Location attributeLocation = locations[0];

						if (attributeLocation != null)
						{
							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.PX1030_DefaultAttibuteToExisitingRecords, attributeLocation));

						}
					}

				}
			}
			return;
        }

        private static bool CheckProperty(IPropertySymbol property, PXContext pxContext)
        {
            var parent = property?.ContainingType;

            if (parent == null || !parent.InheritsFrom(pxContext.PXCacheExtensionType))
                return false;
            return property.Type.TypeKind == TypeKind.Struct ||
                   property.Type.TypeKind == TypeKind.Class ||
                   property.Type.TypeKind == TypeKind.Array;
        }
    }
}
