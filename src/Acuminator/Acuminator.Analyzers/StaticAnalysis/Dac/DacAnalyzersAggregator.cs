using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInDac;
using Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Analyzers.StaticAnalysis.UnderscoresInDac;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.Dac
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DacAnalyzersAggregator : SymbolAnalyzersAggregator<IDacAnalyzer>
    {
        protected override SymbolKind SymbolKind => SymbolKind.NamedType;

        public DacAnalyzersAggregator() : this(null,
			new DacNonAbstractFieldTypeAnalyzer(),
			new ConstructorInDacAnalyzer(),
			new UnderscoresInDacAnalyzer(),
			new ForbiddenFieldsInDacAnalyzer())
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
				return;

			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			var inferredDacModel = DacSemanticModel.InferModel(pxContext, type, context.CancellationToken);

			if (inferredDacModel == null)
				return;

			Parallel.ForEach(_innerAnalyzers, parallelOptions, innerAnalyzer =>
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (innerAnalyzer.ShouldAnalyze(pxContext, inferredDacModel))
				{
					innerAnalyzer.Analyze(context, pxContext, inferredDacModel);
				}
			});
		}
    }
}
