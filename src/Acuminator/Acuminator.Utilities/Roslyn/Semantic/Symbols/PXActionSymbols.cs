using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXActionSymbols
    {
        public INamedTypeSymbol Type { get; }

		public ImmutableArray<IMethodSymbol> SetVisible { get; }
	    public ImmutableArray<IMethodSymbol> SetEnabled { get; }
	    public ImmutableArray<IMethodSymbol> SetCaption { get; }
	    public ImmutableArray<IMethodSymbol> SetTooltip { get; }

        internal PXActionSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXAction).FullName);

	        SetVisible = Type.GetMethods(nameof(PX.Data.PXAction.SetVisible));
			SetEnabled = Type.GetMethods(nameof(PX.Data.PXAction.SetEnabled));
	        SetCaption = Type.GetMethods(nameof(PX.Data.PXAction.SetCaption));
	        SetTooltip = Type.GetMethods(nameof(PX.Data.PXAction.SetTooltip));
        }
    }
}
