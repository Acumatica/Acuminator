using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PXGraphAnalyzer : SymbolAnalyzersAggregator<IPXGraphAnalyzer>
    {
        protected override SymbolKind SymbolKind => SymbolKind.NamedType;

        public PXGraphAnalyzer() : this(null,
            new PXGraphCreationInGraphSemanticModelAnalyzer(),
            new SavingChangesInGraphSemanticModelAnalyzer(),
            new ChangesInPXCacheDuringPXGraphInitializationAnalyzer(),
            new LongOperationInPXGraphDuringInitializationAnalyzer(),
            new LongOperationInDataViewDelegateAnalyzer(),
            new PXActionExecutionInGraphSemanticModelAnalyzer(),
            new DatabaseQueriesInPXGraphInitializationAnalyzer())
        {
        }

        /// <summary>
        /// Constructor for the unit tests.
        /// </summary>
        public PXGraphAnalyzer(CodeAnalysisSettings settings, params IPXGraphAnalyzer[] innerAnalyzers)
            : base(settings, innerAnalyzers)
        {
        }

        protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!(context.Symbol is INamedTypeSymbol type))
            {
                return;
            }

            var inferredGraphs = PXGraphSemanticModel.InferModels(pxContext, type, context.CancellationToken);

            foreach (var innerAnalyzer in _innerAnalyzers)
            {
                foreach (var graph in inferredGraphs)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    innerAnalyzer.Analyze(context, pxContext, settings, graph);
                }
            }
        }
    }
}
