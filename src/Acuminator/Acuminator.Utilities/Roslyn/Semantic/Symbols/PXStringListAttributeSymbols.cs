using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXStringListAttributeSymbols
    {
        public INamedTypeSymbol Type { get; }

		public ImmutableArray<IMethodSymbol> SetList { get; }
	    public ImmutableArray<IMethodSymbol> AppendList { get; }
	    public ImmutableArray<IMethodSymbol> SetLocalizable { get; }

        internal PXStringListAttributeSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXStringListAttribute).FullName);

	        SetList        = Type.GetMethods(nameof(PX.Data.PXStringListAttribute.SetList));
	        AppendList     = Type.GetMethods(nameof(PX.Data.PXStringListAttribute.AppendList));
	        SetLocalizable = Type.GetMethods(nameof(PX.Data.PXStringListAttribute.SetLocalizable));
        }
    }
}
