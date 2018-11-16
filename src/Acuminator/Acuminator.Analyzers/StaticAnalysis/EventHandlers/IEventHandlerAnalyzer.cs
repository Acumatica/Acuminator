using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlers
{
    public interface IEventHandlerAnalyzer : ISymbolAnalyzer
	{
		void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, 
			EventType eventType);

		/// <summary>
		/// Determine if the analyzer should run for event type.
		/// </summary>
		/// <param name="pxContext">Context.</param>
		/// <param name="settings">Options for controlling the analysis.</param>
		/// <param name="eventType">Type of the event.</param>
		/// <returns/>
		bool ShouldAnalyze(PXContext pxContext, CodeAnalysisSettings settings, EventType eventType);
	}
}
