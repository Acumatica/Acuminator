using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
    public class ThrowingExceptionsInLongRunningOperationAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var walker = new WalkerForGraphAnalyzer(context, pxContext, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation);

            CheckProcessingDelegates(pxGraph, walker, context.CancellationToken);
            CheckLongOperationStartDelegates(pxGraph.Symbol, walker, pxContext, context.Compilation, context.CancellationToken);
        }

        private void CheckProcessingDelegates(PXGraphSemanticModel pxGraph, WalkerForGraphAnalyzer walker, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            if (!pxGraph.IsProcessing)
            {
                return;
            }

            var processingViews = pxGraph.Views.Where(v => v.IsProcessing);

            foreach (var viewDel in processingViews)
            {
                var finallyDelegates = viewDel.FinallyProcessDelegates.Where(d => d.Node != null);

                foreach (var finDel in finallyDelegates)
                {
                    cancellation.ThrowIfCancellationRequested();
                    walker.Visit(finDel.Node);
                }

                var parametersDelegates = viewDel.ParametersDelegates.Where(d => d.Node != null);

                foreach (var parDel in parametersDelegates)
                {
                    cancellation.ThrowIfCancellationRequested();
                    walker.Visit(parDel.Node);
                }

                var processDelegates = viewDel.ProcessDelegates.Where(d => d.Node != null);

                foreach (var processDel in processDelegates)
                {
                    cancellation.ThrowIfCancellationRequested();
                    walker.Visit(processDel.Node);
                }
            }
        }

        private void CheckLongOperationStartDelegates(ISymbol symbol, WalkerForGraphAnalyzer walker, PXContext pxContext,
                                                      Compilation compilation, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            var longOperationDelegates = GetLongOperationStartDelegates(symbol, pxContext, compilation, cancellation);

            foreach (var loDel in longOperationDelegates)
            {
                cancellation.ThrowIfCancellationRequested();
                walker.Visit(loDel);
            }
        }

        private IEnumerable<SyntaxNode> GetLongOperationStartDelegates(ISymbol symbol, PXContext pxContext, Compilation compilation,
                                                                       CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            var loDelegateNodeList = new List<SyntaxNode>();
            var declaringNodes = symbol.DeclaringSyntaxReferences
                                 .Select(r => r.GetSyntax(cancellation));

            foreach (var node in declaringNodes)
            {
                cancellation.ThrowIfCancellationRequested();

                var loStartWalker = new StartLongOperationDelegateWalker(pxContext, compilation, cancellation);

                loStartWalker.Visit(node);
                loDelegateNodeList.AddRange(loStartWalker.Delegates);
            }

            return loDelegateNodeList;
        }
    }
}
