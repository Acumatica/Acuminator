using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DAC
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DacAnalyzersAggregator : SymbolAnalyzersAggregator<IDacAnalyzer>
    {
        protected override SymbolKind SymbolKind => SymbolKind.NamedType;

        public DacAnalyzersAggregator() : this(null)
        {
        }

        /// <summary>
        /// Constructor for the unit tests.
        /// </summary>
        public DacAnalyzersAggregator(CodeAnalysisSettings settings, params IDacAnalyzer[] innerAnalyzers) : base(settings, innerAnalyzers)
        {
        }

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!(context.Symbol is INamedTypeSymbol type))
			{
				return;
			}

			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			//var inferredGraphs = PXGraphSemanticModel.InferModels(pxContext, type, context.CancellationToken);
			
			//foreach (var graph in inferredGraphs)
			//{
			//	Parallel.ForEach(_innerAnalyzers, parallelOptions, innerAnalyzer =>
			//	{
			//		context.CancellationToken.ThrowIfCancellationRequested();

			//		if (innerAnalyzer.ShouldAnalyze(pxContext, graph))
			//		{
			//			innerAnalyzer.Analyze(context, pxContext, graph);
			//		}
			//	});
			//}
		}
    }
}
