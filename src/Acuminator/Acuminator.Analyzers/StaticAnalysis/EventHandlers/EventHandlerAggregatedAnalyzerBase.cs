using System.Collections.Immutable;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;



namespace Acuminator.Analyzers.StaticAnalysis.EventHandlers
{
	/// <summary>
	/// Base class for aggregated event handler analyzers.
	/// </summary>
	public abstract class EventHandlerAggregatedAnalyzerBase : IEventHandlerAnalyzer
	{
		public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public abstract void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, EventType eventType);

		public virtual bool ShouldAnalyze(PXContext pxContext, CodeAnalysisSettings settings, EventType eventType) => true;
	}
}
