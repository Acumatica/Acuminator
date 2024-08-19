
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Dac
{
	/// <summary>
	/// Base class for aggregated DAC analyzers.
	/// </summary>
	public abstract class DacAggregatedAnalyzerBase : IDacAnalyzer
	{
		public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public abstract void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac);

		public virtual bool ShouldAnalyze(PXContext pxContext, [NotNullWhen(returnValue: true)] DacSemanticModel dac) => dac?.IsInSource == true;
	}
}
