using System.Collections.Immutable;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

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
	        SetVisible = Type.GetMethods(DelegateNames.SetVisible).ToImmutableArray();
	        SetEnabled = Type.GetMethods(DelegateNames.SetEnabled).ToImmutableArray();
	        SetCaption = Type.GetMethods(DelegateNames.SetCaption).ToImmutableArray();
	        SetTooltip = Type.GetMethods(DelegateNames.SetTooltip).ToImmutableArray();
	        Press      = Type.GetMethods(DelegateNames.Press).ToImmutableArray();
        }
    }
}
