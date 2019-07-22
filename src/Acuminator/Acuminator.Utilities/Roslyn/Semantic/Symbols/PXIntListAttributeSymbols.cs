using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXIntListAttributeSymbols : SymbolsSetForTypeBase
    {
		public ImmutableArray<IMethodSymbol> SetList { get; }

        internal PXIntListAttributeSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXIntListAttribute)
        {
	        SetList = Type.GetMethods(DelegateNames.SetList);
        }
    }
}
