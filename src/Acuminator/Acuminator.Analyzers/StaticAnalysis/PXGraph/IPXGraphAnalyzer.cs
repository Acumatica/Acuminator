#nullable enable

using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
	public interface IPXGraphAnalyzer : ISymbolAnalyzer
	{
		/// <summary>
		/// Run the analysis for the graph semantic model.
		/// </summary>
		/// <param name="context">Symbol analysis context.</param>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="pxGraph">The graph semantic model.</param>
		void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph);

		/// <summary>
		/// Determine if the analyzer should run on graph.
		/// </summary>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="pxGraph">The graph semantic model.</param>
		/// <returns/>
		bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph);
	}
}
