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

	        SetVisible = Type.GetMethods(Types.PXActionNames.SetVisible);
	        SetEnabled = Type.GetMethods(Types.PXActionNames.SetEnabled);
	        SetCaption = Type.GetMethods(Types.PXActionNames.SetCaption);
	        SetTooltip = Type.GetMethods(Types.PXActionNames.SetTooltip);
	        Press      = Type.GetMethods(Types.PXActionNames.Press);
        }
    }
}
