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
        /// Indicates whether the data view is processing data view
        /// </summary>
        public bool IsProcessing { get; }

        /// <summary>
        /// The symbol of the data view declaration
        /// </summary>
        public ISymbol Symbol { get; }

        /// <summary>
        /// The type of the data view symbol
        /// </summary>
        public INamedTypeSymbol Type { get; }

        public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext)
        {
            symbol.ThrowOnNull(nameof(symbol));
            type.ThrowOnNull(nameof(type));
            pxContext.ThrowOnNull(nameof(pxContext));

            Symbol = symbol;
            Type = type;
            IsProcessing = type.IsProcessingView(pxContext);
        }

        public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext, DataViewInfo baseInfo)
            : this(symbol, type, pxContext)
        {
            baseInfo.ThrowOnNull();

            Base = baseInfo;
        }
    }
}
