using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
    public class ThrowingExceptionsInActionHandlersAnalyzer : PXGraphAggregatedAnalyzerBase
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1090_ThrowingSetupNotEnteredExceptionInActionHandlers);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var walker = new WalkerForGraphAnalyzer(context, pxContext, Descriptors.PX1090_ThrowingSetupNotEnteredExceptionInActionHandlers);
            var delegateNodes = pxGraph.DeclaredActionHandlers
								.Where(h => h.Node != null)
                                .Select(h => h.Node);

            foreach (var node in delegateNodes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(node);
            }
        }
    }
}
