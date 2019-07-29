using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXUIFieldAttributeSymbols : SymbolsSetForTypeBase
    {
		public ImmutableArray<IMethodSymbol> SetVisible { get; }
	    public ImmutableArray<IMethodSymbol> SetVisibility { get; }
	    public ImmutableArray<IMethodSymbol> SetEnabled { get; }
	    public ImmutableArray<IMethodSymbol> SetRequired { get; }
	    public ImmutableArray<IMethodSymbol> SetReadOnly { get; }
	    public ImmutableArray<IMethodSymbol> SetDisplayName { get; }
	    public ImmutableArray<IMethodSymbol> SetNeutralDisplayName { get; }

        internal PXUIFieldAttributeSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXUIFieldAttribute)
        {
	        SetVisible            = Type.GetMethods(DelegateNames.SetVisible);
	        SetVisibility         = Type.GetMethods(DelegateNames.SetVisibility);
	        SetEnabled            = Type.GetMethods(DelegateNames.SetEnabled);
	        SetRequired           = Type.GetMethods(DelegateNames.SetRequired);
	        SetReadOnly           = Type.GetMethods(DelegateNames.SetReadOnly);
	        SetDisplayName        = Type.GetMethods(DelegateNames.SetDisplayName);
	        SetNeutralDisplayName = Type.GetMethods(DelegateNames.SetNeutralDisplayName);
        }
    }
}
