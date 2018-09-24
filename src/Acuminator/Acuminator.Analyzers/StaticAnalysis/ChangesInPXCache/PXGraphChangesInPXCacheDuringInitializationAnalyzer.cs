using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache
{
    public class PXGraphChangesInPXCacheDuringInitializationAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
        {
            Walker walker = new Walker(context, pxContext, Descriptors.PX1059_PXGraphChangesPXCacheDuringInitialization);

            foreach (GraphInitializerInfo initializer in pxGraph.Initializers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(initializer.Node);
            }
        }
    }
}
