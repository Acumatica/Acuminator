using Microsoft.CodeAnalysis;
using System.Linq;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectBaseSymbols
    {
        public INamedTypeSymbol Type { get; }
        public IFieldSymbol View { get; }

        internal PXSelectBaseSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(TypeFullNames.PXSelectBase);
            View = Type.GetMembers(DelegateNames.View)
                   .OfType<IFieldSymbol>()
                   .First();
        }
    }
}
