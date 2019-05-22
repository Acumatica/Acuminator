using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using static Acuminator.Utilities.Common.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXProcessingBaseSymbols
    {
        private const string SetParametersDelegateName = "SetParametersDelegate";
        private const string SetProcessDelegateName = "SetProcessDelegate";

        public INamedTypeSymbol Type { get; }
        public IMethodSymbol SetParametersDelegate { get; }
        public ImmutableArray<IMethodSymbol> SetProcessDelegate { get; }

        internal PXProcessingBaseSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXProcessingBase);
            SetParametersDelegate = Type.GetMethods(Types.PXProcessingBaseDelegates.SetParameters).First();
            SetProcessDelegate = Type.GetMethods(Types.PXProcessingBaseDelegates.SetProcess);
        }
    }
}
