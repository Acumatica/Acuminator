using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
    public class LongOperationInDataViewDelegateAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1080_DataViewDelegateLongOperationStart);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            StartLongOperationWalker walker = new StartLongOperationWalker(context, pxContext, Descriptors.PX1080_DataViewDelegateLongOperationStart);

            foreach (DataViewDelegateInfo del in pxGraph.ViewDelegates)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(del.Node);
            }
        }
    }
}
