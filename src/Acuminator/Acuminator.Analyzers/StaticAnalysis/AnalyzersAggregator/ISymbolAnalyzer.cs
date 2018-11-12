using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator
{
    public interface ISymbolAnalyzer
    {
        ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		bool ShouldAnalyze(PXContext pxContext, CodeAnalysisSettings settings);
	}
}
