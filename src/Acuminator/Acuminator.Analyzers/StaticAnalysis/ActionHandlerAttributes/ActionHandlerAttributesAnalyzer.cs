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
            context.CancellationToken.ThrowIfCancellationRequested();

            var declaredActionHandlers = pxGraph.ActionHandlers
                .Where(h => pxGraph.Symbol.Equals(h.Symbol?.ContainingType));

            foreach (var actionHandler in pxGraph.ActionHandlers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var attributes = actionHandler.Symbol.GetAttributes();
                //attributes[0].AttributeClass
            }
        }
    }
}
