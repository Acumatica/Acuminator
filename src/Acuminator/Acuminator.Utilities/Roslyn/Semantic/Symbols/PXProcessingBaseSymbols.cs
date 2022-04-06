using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXProcessingBaseSymbols : SymbolsSetForTypeBase
    {
        public IMethodSymbol SetParametersDelegate { get; }

        public ImmutableArray<IMethodSymbol> SetProcessDelegate { get; }

        internal PXProcessingBaseSymbols(Compilation compilation) : base(compilation, typeName: TypeFullNames.PXProcessingBase)
        {
            SetParametersDelegate = Type.GetMethods(DelegateNames.SetParameters).First();
            SetProcessDelegate = Type.GetMethods(DelegateNames.SetProcessDelegate);
        }
    }
}
