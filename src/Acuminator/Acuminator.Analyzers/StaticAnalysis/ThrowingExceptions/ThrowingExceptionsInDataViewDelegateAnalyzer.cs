using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
    public class ThrowingExceptionsInDataViewDelegateAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var walker = new Walker(context, pxContext);

            foreach (var viewDel in pxGraph.ViewDelegates)
            {
                walker.Visit(viewDel.Node);
            }
        }

        private class Walker : WalkerBase
        {
            public Walker(SymbolAnalysisContext context, PXContext pxContext)
                : base(context, pxContext)
            {
            }

            public override void VisitThrowStatement(ThrowStatementSyntax node)
            {
                ThrowIfCancellationRequested();

                if (IsPXSetupNotEnteredException(node))
                {
                    ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization, node);
                }
                else
                {
                    base.VisitThrowStatement(node);
                }
            }
        }
    }
}
