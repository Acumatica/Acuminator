using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
    public class DatabaseQueriesInPXGraphInitializationAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            if (!settings.IsvSpecificAnalyzersEnabled)
            {
                return;
            }

            var dbQueriesWalker = new Walker(context, pxContext, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization);

            foreach (var initializer in pxGraph.Initializers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                dbQueriesWalker.Visit(initializer.Node);
            }
        }
    }
}
