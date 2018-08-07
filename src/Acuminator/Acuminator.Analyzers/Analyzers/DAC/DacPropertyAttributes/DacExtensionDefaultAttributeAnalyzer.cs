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

            foreach (var attribute in attributes)
            {
                var typesHierarchy = attribute.AttributeClass.GetBaseTypesAndThis();
                if (typesHierarchy.Contains())
                {
                    return;
                }
               // var allAttr = typesHierarchy.SelectMany(type => type.GetAttributes);
            }


            /*
            var attributesWithInfo = DacPropertyAttributesAnalyzer.GetFieldAttributesInfos(pxContext, attributes, symbolContext.CancellationToken);

            if (symbolContext.CancellationToken.IsCancellationRequested || attributesWithInfo.IsNullOrEmpty())
                return;

            if (attributesWithInfo.Count > 1)
            {
                var locationTasks = attributesWithInfo.Select(info => DacPropertyAttributesAnalyzer.GetAttributeLocationAsync(info.Attribute, symbolContext.CancellationToken));
                Location[] attributeLocations = await Task.WhenAll(locationTasks);

                foreach (Location attrLocation in attributeLocations)
                {
                    if (attrLocation == null)
                        continue;

                    symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1030_DefaultAttibuteToExisitingRecords, attrLocation));
                }
            }
            else
            {
                var (fieldAttribute, fieldAttrInfo) = attributesWithInfo[0];

#pragma warning disable CS4014
               // CheckAttributeAndPropertyTypesForCompatibility(fieldAttribute, fieldAttrInfo, pxContext, symbolContext);
#pragma warning restore CS4014
            }
           */ 
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
