using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class DataViewInfo : GraphNodeSymbolItem<ISymbol>
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
        /// The type of the data view symbol
        /// </summary>
        public INamedTypeSymbol Type { get; }

        /// <summary>
        /// The process delegates
        /// </summary>
        public ImmutableArray<DataViewDelegateInfo> ProcessDelegates { get; }

        /// <summary>
        /// The parameters process delegate
        /// </summary>
        public DataViewDelegateInfo ParametersDelegate { get; }

        /// <summary>
        /// The finally process delegate
        /// </summary>
        public DataViewDelegateInfo FinallyProcessDelegate { get; }

        public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext)
            : base(symbol)
        {
            type.ThrowOnNull(nameof(type));
            pxContext.ThrowOnNull(nameof(pxContext));

            Type = type;
            IsProcessing = type.IsProcessingView(pxContext);
        }

        public DataViewInfo(ISymbol symbol, INamedTypeSymbol type, PXContext pxContext, DataViewInfo baseInfo)
            : this(symbol, type, pxContext)
        {
            baseInfo.ThrowOnNull();

            Base = baseInfo;
        }

        public void InitProcessingDelegatesInfo()
        {
        }
    }
}
