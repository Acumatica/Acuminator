using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

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
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXUIFieldAttribute).FullName);

	        SetVisible            = Type.GetMethods(nameof(PX.Data.PXUIFieldAttribute.SetVisible));
	        SetVisibility         = Type.GetMethods(nameof(PX.Data.PXUIFieldAttribute.SetVisibility));
	        SetEnabled            = Type.GetMethods(nameof(PX.Data.PXUIFieldAttribute.SetEnabled));
	        SetRequired           = Type.GetMethods(nameof(PX.Data.PXUIFieldAttribute.SetRequired));
	        SetReadOnly           = Type.GetMethods(nameof(PX.Data.PXUIFieldAttribute.SetReadOnly));
	        SetDisplayName        = Type.GetMethods(nameof(PX.Data.PXUIFieldAttribute.SetDisplayName));
	        SetNeutralDisplayName = Type.GetMethods(nameof(PX.Data.PXUIFieldAttribute.SetNeutralDisplayName));
        }
    }
}
