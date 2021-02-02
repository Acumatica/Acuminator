using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    /// <summary>
    /// A referential integrity symbols related to Acumatica PK/FK API.
    /// </summary>
    public class PXReferentialIntegritySymbols : SymbolsSetBase
    {
        public static ImmutableHashSet<string> ForeignKeyContainerNames { get; } =
            new HashSet<string>
            {
                TypeNames.ReferentialIntegrity.AsSimpleKeyName,
                TypeNames.ReferentialIntegrity.ForeignKeyOfName,
                TypeNames.ReferentialIntegrity.CompositeKey
            }
            .ToImmutableHashSet();

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

        /// <summary>
        /// Gets the generic foreign key to the parent DAC interface derived from <see cref="IForeignKey"/>. Contains information about parent DAC referenced by the foreign key.
        /// For earlier versions of Acumatica (2019R1) the interface is not defined so the symbol can be null.
        /// </summary>
        /// <value>
        /// The generic foreign key to the parent DAC interface
        /// </value>
        public INamedTypeSymbol IForeignKeyTo1 { get; }

        public INamedTypeSymbol KeysRelation { get; }

        public INamedTypeSymbol PrimaryKeyOf => Compilation.GetTypeByMetadataName(TypeFullNames.PrimaryKeyOf);

        public INamedTypeSymbol CompositeKey2 => Compilation.GetTypeByMetadataName(TypeFullNames.CompositeKey2);

        internal PXReferentialIntegritySymbols(Compilation compilation) : base(compilation)
        {
            IPrimaryKey = Compilation.GetTypeByMetadataName(TypeFullNames.IPrimaryKey);
            IForeignKey = Compilation.GetTypeByMetadataName(TypeFullNames.IForeignKey);
            IForeignKeyTo1 = Compilation.GetTypeByMetadataName(TypeFullNames.IForeignKeyTo1);
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
