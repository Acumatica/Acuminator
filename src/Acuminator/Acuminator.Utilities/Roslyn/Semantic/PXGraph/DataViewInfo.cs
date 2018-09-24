using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public readonly struct DataViewInfo
    {
        public ISymbol Symbol { get; }
        public INamedTypeSymbol Type { get; }

        public DataViewInfo(ISymbol symbol, INamedTypeSymbol type)
        {
            Symbol = symbol;
            Type = type;
        }
    }
}
