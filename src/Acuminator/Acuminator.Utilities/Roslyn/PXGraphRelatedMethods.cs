using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Utilities.Roslyn
{
    public class PXGraphRelatedMethods
    {
        private const string _createInstanceMethodsName = "CreateInstance";
        private const string _instanceCreatedEventsAddHandler = "AddHandler";

        public ImmutableArray<IMethodSymbol> CreateInstance { get; private set; }
        public IMethodSymbol InstanceCreatedEventsAddHandler { get; private set; }

        internal PXGraphRelatedMethods(PXContext pxContext)
        {
            CreateInstance = pxContext.PXGraphType.GetMembers(_createInstanceMethodsName)
                             .OfType<IMethodSymbol>()
                             .ToImmutableArray();

            InstanceCreatedEventsAddHandler = pxContext.InstanceCreatedEvents.GetMembers(_instanceCreatedEventsAddHandler)
                                              .OfType<IMethodSymbol>()
                                              .First();
        }
    }
}
