using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXIntListAttributeSymbols
    {
        public INamedTypeSymbol Type { get; }

		public ImmutableArray<IMethodSymbol> SetList { get; }

        internal PXIntListAttributeSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXIntListAttribute);

	        SetList = Type.GetMethods(Types.PXIntListAttributeDelegates.SetList);
        }
    }
}
