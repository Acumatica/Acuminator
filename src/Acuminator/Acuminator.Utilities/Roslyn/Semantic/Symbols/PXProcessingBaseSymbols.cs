using Microsoft.CodeAnalysis;
using PX.Data;
using System.Collections.Immutable;
using System.Linq;

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
            Type = compilation.GetTypeByMetadataName(typeof(PXProcessingBase<>).FullName);
            SetParametersDelegate = Type.GetMethods(SetParametersDelegateName).First();
            SetProcessDelegate = Type.GetMethods(SetProcessDelegateName);
        }
    }
}
