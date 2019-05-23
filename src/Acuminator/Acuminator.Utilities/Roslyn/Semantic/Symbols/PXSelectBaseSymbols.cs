using Microsoft.CodeAnalysis;
using System.Linq;
using static Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectBaseSymbols
    {
        public INamedTypeSymbol Type { get; }
        public IFieldSymbol View { get; }

        internal PXSelectBaseSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXSelectBase);
            View = Type.GetMembers(Types.PXSelectBaseDelegates.View)
                   .OfType<IFieldSymbol>()
                   .First();
        }
    }
}
