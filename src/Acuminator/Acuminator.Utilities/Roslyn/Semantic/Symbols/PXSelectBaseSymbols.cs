using Microsoft.CodeAnalysis;
using PX.Data;
using System.Linq;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXSelectBaseSymbols
    {
        public INamedTypeSymbol Type { get; }
        public IFieldSymbol View { get; }

        internal PXSelectBaseSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
            View = Type.GetMembers(nameof(PXSelectBase.View))
                   .OfType<IFieldSymbol>()
                   .First();
        }
    }
}
