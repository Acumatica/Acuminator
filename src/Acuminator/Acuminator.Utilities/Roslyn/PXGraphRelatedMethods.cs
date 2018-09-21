using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Utilities.Roslyn
{
    public class PXGraphRelatedMethods
    {
        private const string CreateInstanceMethodsName = "CreateInstance";
        private const string InstanceCreatedEventsAddHandlerName = "AddHandler";

        public ImmutableArray<IMethodSymbol> CreateInstance { get; }
        public IMethodSymbol InstanceCreatedEventsAddHandler { get; }

        internal PXGraphRelatedMethods(PXContext pxContext)
        {
            CreateInstance = pxContext.PXGraphType.GetMembers(CreateInstanceMethodsName)
                             .OfType<IMethodSymbol>()
                             .ToImmutableArray();

            InstanceCreatedEventsAddHandler = pxContext.InstanceCreatedEvents.GetMembers(InstanceCreatedEventsAddHandlerName)
                                              .OfType<IMethodSymbol>()
                                              .First();
        }
    }
}
