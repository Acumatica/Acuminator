using System;
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
        /// The maximum size of the DAC primary key.
        /// </summary>
        public const int MaxPrimaryKeySize = 8;

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

        public INamedTypeSymbol PrimaryKeyOf => Compilation.GetTypeByMetadataName(TypeFullNames.PrimaryKeyOf);

        internal PXReferentialIntegritySymbols(Compilation compilation) : base(compilation)
        {
            IPrimaryKey = Compilation.GetTypeByMetadataName(TypeFullNames.IPrimaryKey);
            IForeignKey = Compilation.GetTypeByMetadataName(TypeFullNames.IForeignKey);
            KeysRelation = Compilation.GetTypeByMetadataName(TypeFullNames.KeysRelation);
        }

        public INamedTypeSymbol GetPrimaryKeyBy_TypeSymbol(int arity)
        {
            if (arity <= 0 || arity > MaxPrimaryKeySize)
                throw new ArgumentOutOfRangeException(nameof(arity));

            string primaryKeyByTypeName = $"{TypeFullNames.PrimaryKeyOfBy}`{arity}";
            return Compilation.GetTypeByMetadataName(primaryKeyByTypeName);
        }
    }
}
