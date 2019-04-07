using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
    public class DatabaseQueriesInPXGraphInitializationAnalyzer : PXGraphAggregatedAnalyzerBase
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
        {
            var dbQueriesWalker = new Walker(context, pxContext, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization);

            foreach (var initializer in pxGraph.Initializers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                dbQueriesWalker.Visit(initializer.Node);
            }
        }		
	}
}
