#nullable enable

using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
	/// <summary>
	/// Base class for aggregated graph analyzers with events.
	/// </summary>
	public abstract class PXGraphWithGraphEventsAggregatedAnalyzerBase : IPXGraphWithGraphEventsAnalyzer
	{
		/// <inheritdoc cref="ISymbolAnalyzer.SupportedDiagnostics"/>
		public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		/// <inheritdoc cref="IPXGraphWithGraphEventsAnalyzer.ShouldAnalyze(PXContext, PXGraphSemanticModel)"/>
		public virtual bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel pxGraph) =>
			pxGraph != null;

		/// <inheritdoc cref="IPXGraphWithGraphEventsAnalyzer.ShouldAnalyze(PXContext, PXGraphEventSemanticModel)"/>
		public virtual bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel pxGraphWithEvents) => 
			pxGraphWithEvents != null;

		public abstract void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel pxGraphWithEvents);
	}
}
