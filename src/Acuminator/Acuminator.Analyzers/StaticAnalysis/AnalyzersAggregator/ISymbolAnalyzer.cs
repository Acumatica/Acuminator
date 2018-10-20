using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator
{
    public interface ISymbolAnalyzer
    {
        ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
    }
}
