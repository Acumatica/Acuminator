using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class DataViewInfo
    {
        /// <summary>
        /// The overriden item if any
        /// </summary>
        public DataViewInfo Base { get; }

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

        public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, DataViewInfo baseInfo)
            : this(symbol, type)
        {
            baseInfo.ThrowOnNull();

            Base = baseInfo;
        }
    }
}
