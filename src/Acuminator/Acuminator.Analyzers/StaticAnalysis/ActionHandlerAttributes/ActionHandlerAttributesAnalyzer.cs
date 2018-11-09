using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.ActionHandlerAttributes
{
    public class ActionHandlerAttributesAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1092_MissingAttributesOnActionHandler);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            foreach (var actionHandler in pxGraph.ActionHandlers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (actionHandler.Symbol == null)
                {
                    continue;
                }

                var attributes = actionHandler.Symbol.GetAttributes();
                var pxUiFieldAttributeType = pxContext.AttributeTypes.PXUIFieldAttribute.Type;
                var pxButtonAttributeType = pxContext.AttributeTypes.PXButtonAttribute;
                var pxUiFieldAttributeExists = false;
                var pxButtonAttributeExists = false;

                foreach (var attr in attributes)
                {
                    if (pxUiFieldAttributeType.Equals(attr.AttributeClass))
                    {
                        pxUiFieldAttributeExists = true;
                    }

                    if (pxButtonAttributeType.Equals(attr.AttributeClass))
                    {
                        pxButtonAttributeExists = true;
                    }
                }

                //attributes[0].AttributeClass
                //pxContext.AttributeTypes.PXUIFieldAttribute.Type;
                //pxContext.AttributeTypes.PXButtonAttribute;
            }
        }
    }
}
