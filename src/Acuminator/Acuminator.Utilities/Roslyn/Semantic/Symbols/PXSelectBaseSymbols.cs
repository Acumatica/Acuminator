using Microsoft.CodeAnalysis;
using System.Linq;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectBaseSymbols : SymbolsSetForTypeBase
    {
        public IFieldSymbol View { get; }

        internal PXSelectBaseSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXSelectBase)
        {
            View = Type.GetMembers(DelegateNames.View)
                   .OfType<IFieldSymbol>()
                   .First();
        }
    }
}
