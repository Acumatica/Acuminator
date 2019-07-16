using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.Dac
{

	/// <summary>
	/// Base class for aggregated DAC analyzers.
	/// </summary>
	public abstract class DacAggregatedAnalyzerBase : IDacAnalyzer
	{
		public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public abstract void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac);

		public virtual bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) => dac != null && dac.DacType != DacType.None;
	}
}
