using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic
{
    public class UiPresentationLogicInActionHandlersAnalyzer : PXGraphAggregatedAnalyzerBase
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1089_UiPresentationLogicInActionDelegates);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var walker = new Walker(context, pxContext, Descriptors.PX1089_UiPresentationLogicInActionDelegates);
            var delegateNodes = pxGraph.ActionHandlers
                                .Where(h => h.Node != null)
                                .Select(h => h.Node);

            foreach (var node in delegateNodes)
            {
                walker.Visit(node);
            }
        }
    }
}
