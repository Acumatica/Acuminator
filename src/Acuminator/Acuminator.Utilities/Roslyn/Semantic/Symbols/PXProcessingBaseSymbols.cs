using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXProcessingBaseSymbols
    {
        public INamedTypeSymbol Type { get; }
        public IMethodSymbol SetParametersDelegate { get; }
        public ImmutableArray<IMethodSymbol> SetProcessDelegate { get; }

        internal PXProcessingBaseSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(TypeFullNames.PXProcessingBase);
            SetParametersDelegate = Type.GetMethods(DelegateNames.SetParameters).First();
            SetProcessDelegate = Type.GetMethods(DelegateNames.SetProcess);
        }
    }
}
