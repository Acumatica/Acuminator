using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXActionSymbols
    {
        public INamedTypeSymbol Type { get; }

		public ImmutableArray<IMethodSymbol> SetVisible { get; }
	    public ImmutableArray<IMethodSymbol> SetEnabled { get; }
	    public ImmutableArray<IMethodSymbol> SetCaption { get; }
	    public ImmutableArray<IMethodSymbol> SetTooltip { get; }
	    public ImmutableArray<IMethodSymbol> Press { get; }

        internal PXActionSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXAction);

	        SetVisible = Type.GetMethods(Types.PXActionDelegates.SetVisible);
	        SetEnabled = Type.GetMethods(Types.PXActionDelegates.SetEnabled);
	        SetCaption = Type.GetMethods(Types.PXActionDelegates.SetCaption);
	        SetTooltip = Type.GetMethods(Types.PXActionDelegates.SetTooltip);
	        Press      = Type.GetMethods(Types.PXActionDelegates.Press);
        }
    }
}
