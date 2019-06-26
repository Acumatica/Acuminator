using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Dac
{
    /// <summary>
    /// Interface for DAC aggregated analyzers.
    /// </summary>
    public interface IDacAnalyzer : ISymbolAnalyzer
    {
		void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac);

		/// <summary>
		/// Determine if the analyzer should run on DAC.
		/// </summary>
		/// <param name="pxContext"> Context.</param>
		/// <param name="dac"> The dac semantic model.</param>
		/// <returns/>
		bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac);
	}
}
