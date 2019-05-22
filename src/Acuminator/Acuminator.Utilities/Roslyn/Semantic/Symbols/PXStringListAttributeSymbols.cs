using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants.Types;

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
            Type = compilation.GetTypeByMetadataName(PXStringListAttribute);

	        SetList        = Type.GetMethods(PXStringListAttributeDelegates.SetList);
	        AppendList     = Type.GetMethods(PXStringListAttributeDelegates.AppendList));
	        SetLocalizable = Type.GetMethods(PXStringListAttributeDelegates.SetLocalizable));
        }
    }
}
