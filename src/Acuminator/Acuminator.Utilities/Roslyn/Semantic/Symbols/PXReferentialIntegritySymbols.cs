using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    /// <summary>
    /// A referential integrity symbols related to Acumatica PK/FK API.
    /// </summary>
    public class PXReferentialIntegritySymbols : SymbolsSetBase
    {
        /// <summary>
        /// Gets the primary key interface.
        /// </summary>
        /// <value>
        /// The primary key interface.
        /// </value>
        public INamedTypeSymbol IPrimaryKey { get; }

        /// <summary>
        /// Gets the foreign key interface. For earlier versions of Acumatica (2019R1) is not defined so it can be null.
        /// </summary>
        /// <value>
        /// The foreign key interface.
        /// </value>
        public INamedTypeSymbol IForeignKey { get; }

        public INamedTypeSymbol KeysRelation { get; }

        internal PXReferentialIntegritySymbols(Compilation compilation) : base(compilation)
        {
            IPrimaryKey = Compilation.GetTypeByMetadataName(TypeFullNames.IPrimaryKey);
            IForeignKey = Compilation.GetTypeByMetadataName(TypeFullNames.IForeignKey);
            KeysRelation = Compilation.GetTypeByMetadataName(TypeFullNames.KeysRelation);
        }
    }
}
