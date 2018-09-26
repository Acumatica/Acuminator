using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXIntListAttributeSymbols
    {
        public INamedTypeSymbol Type { get; }

		public ImmutableArray<IMethodSymbol> SetList { get; }

        internal PXIntListAttributeSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXIntListAttribute).FullName);

	        SetList = Type.GetMethods(nameof(PX.Data.PXIntListAttribute.SetList));
        }
    }
}
