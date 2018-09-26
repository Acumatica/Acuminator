using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public readonly struct DataViewInfo
    {
        /// <summary>
        /// The symbol of the data view declaration
        /// </summary>
        public ISymbol Symbol { get; }

        /// <summary>
        /// The type of the data view symbol
        /// </summary>
        public INamedTypeSymbol Type { get; }

        public DataViewInfo(ISymbol symbol, INamedTypeSymbol type)
        {
            symbol.ThrowOnNull(nameof(symbol));
            type.ThrowOnNull(nameof(type));

            Symbol = symbol;
            Type = type;
        }
    }
}
