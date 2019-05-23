using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXUIFieldAttributeSymbols
    {
        public INamedTypeSymbol Type { get; }

		public ImmutableArray<IMethodSymbol> SetVisible { get; }
	    public ImmutableArray<IMethodSymbol> SetVisibility { get; }
	    public ImmutableArray<IMethodSymbol> SetEnabled { get; }
	    public ImmutableArray<IMethodSymbol> SetRequired { get; }
	    public ImmutableArray<IMethodSymbol> SetReadOnly { get; }
	    public ImmutableArray<IMethodSymbol> SetDisplayName { get; }
	    public ImmutableArray<IMethodSymbol> SetNeutralDisplayName { get; }

        internal PXUIFieldAttributeSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXUIFieldAttribute);

	        SetVisible            = Type.GetMethods(Types.PXUIFieldAttributeSetVisible);
	        SetVisibility         = Type.GetMethods(Types.PXUIFieldAttributeSetVisibility);
	        SetEnabled            = Type.GetMethods(Types.PXUIFieldAttributeSetEnabled);
	        SetRequired           = Type.GetMethods(Types.PXUIFieldAttributeSetRequired);
	        SetReadOnly           = Type.GetMethods(Types.PXUIFieldAttributeSetReadOnly);
	        SetDisplayName        = Type.GetMethods(Types.PXUIFieldAttributeSetDisplayName);
	        SetNeutralDisplayName = Type.GetMethods(Types.PXUIFieldAttributeSetNeutralDisplayName);
        }
    }
}
