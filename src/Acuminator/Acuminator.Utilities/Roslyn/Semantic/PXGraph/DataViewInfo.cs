using System.Diagnostics;
using System.Linq;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;


namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
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

		public ITypeSymbol ViewDac => Type.TypeArguments.FirstOrDefault();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => $"Symbol: {Symbol.Name} | Type: {Type.ToString()}";

		public DataViewInfo(ISymbol symbol, INamedTypeSymbol type)
        {
            symbol.ThrowOnNull(nameof(symbol));
            type.ThrowOnNull(nameof(type));

            Symbol = symbol;
            Type = type;
        }
    }
}
