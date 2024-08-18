#nullable enable

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator
{
    public interface ISymbolAnalyzer
    {
        ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
	}
}
