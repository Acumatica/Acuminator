#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
	/// <summary>
	/// Aggregator analyzer for graph analyzers that reuses the enriched graph semantic model with graph events.
	/// </summary>
	public sealed class PXGraphWithGraphEventsAggregatorAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		private readonly ImmutableArray<IPXGraphWithGraphEventsAnalyzer> _innerAnalyzers;

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			graph != null && !_innerAnalyzers.IsDefaultOrEmpty;

		public PXGraphWithGraphEventsAggregatorAnalyzer(params IPXGraphWithGraphEventsAnalyzer[] innerAnalyzers)
		{
			_innerAnalyzers		 = ImmutableArray.CreateRange(innerAnalyzers);
			SupportedDiagnostics = ImmutableArray.CreateRange(innerAnalyzers.SelectMany(a => a.SupportedDiagnostics));
		}

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraphOrExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var effectiveAnalyzers = _innerAnalyzers.Where(analyzer => analyzer.ShouldAnalyze(pxContext, pxGraphOrExtension))
													.ToList(capacity: _innerAnalyzers.Length);
			if (effectiveAnalyzers.Count == 0)
				return;

			var graphOrExtensionWithEvents = PXGraphEventSemanticModel.EnrichGraphModelWithEvents(pxGraphOrExtension, context.CancellationToken);

			foreach (IPXGraphWithGraphEventsAnalyzer innerAnalyzer in effectiveAnalyzers)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (innerAnalyzer.ShouldAnalyze(pxContext, graphOrExtensionWithEvents))
				{
					innerAnalyzer.Analyze(context, pxContext, graphOrExtensionWithEvents);
				}
			}
		}
	}
}
