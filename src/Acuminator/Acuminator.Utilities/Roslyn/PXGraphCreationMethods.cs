using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Acuminator.Utilities.Roslyn
{
    public class PXGraphCreationMethods
    {
        private const string _createInstanceMethodsName = "CreateInstance";

        public ImmutableArray<IMethodSymbol> CreateInstance { get; private set; }

        internal PXGraphCreationMethods(PXContext pxContext)
        {
            CreateInstance = pxContext.PXGraphType.GetMembers(_createInstanceMethodsName)
                             .OfType<IMethodSymbol>()
                             .ToImmutableArray();
        }
    }
}
