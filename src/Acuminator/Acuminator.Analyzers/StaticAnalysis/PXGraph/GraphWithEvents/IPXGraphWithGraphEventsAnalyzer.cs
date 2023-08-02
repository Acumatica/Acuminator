#nullable enable

using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
	public interface IPXGraphWithGraphEventsAnalyzer : ISymbolAnalyzer
	{
		/// <summary>
		/// Determine if the analyzer should run on graph.
		/// </summary>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="pxGraphWithEvents">The normal graph semantic model without graph events for preliminary checks.</param>
		/// <returns/>
		bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel pxGraphWithoutEvents);

		/// <summary>
		/// Determine if the analyzer should run on graph.
		/// </summary>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="pxGraphWithEvents">The enhanced graph semantic model with graph events.</param>
		/// <returns/>
		bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel pxGraphWithEvents);

		/// <summary>
		/// Run the analysis for the enhanced graph semantic model.
		/// </summary>
		/// <param name="context">Symbol analysis context.</param>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="pxGraph">The graph semantic model.</param>
		void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel pxGraphWithEvents);
	}
}