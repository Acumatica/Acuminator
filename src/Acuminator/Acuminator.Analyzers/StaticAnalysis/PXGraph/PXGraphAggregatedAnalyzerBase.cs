using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{

	/// <summary>
	/// Base class for aggregated graph analyzers.
	/// </summary>
	public abstract class PXGraphAggregatedAnalyzerBase : IPXGraphAnalyzer
	{
		public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public abstract void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph);

		public virtual bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) => true;
	}
}
