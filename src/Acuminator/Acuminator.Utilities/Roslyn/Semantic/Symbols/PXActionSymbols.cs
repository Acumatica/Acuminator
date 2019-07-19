using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXActionSymbols : SymbolsSetForTypeBase
    {
		public ImmutableArray<IMethodSymbol> SetVisible { get; }
	    public ImmutableArray<IMethodSymbol> SetEnabled { get; }
	    public ImmutableArray<IMethodSymbol> SetCaption { get; }
	    public ImmutableArray<IMethodSymbol> SetTooltip { get; }
	    public ImmutableArray<IMethodSymbol> Press { get; }

        internal PXActionSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXAction)
        {
	        SetVisible = Type.GetMethods(DelegateNames.SetVisible);
	        SetEnabled = Type.GetMethods(DelegateNames.SetEnabled);
	        SetCaption = Type.GetMethods(DelegateNames.SetCaption);
	        SetTooltip = Type.GetMethods(DelegateNames.SetTooltip);
	        Press      = Type.GetMethods(DelegateNames.Press);
        }
    }
}
