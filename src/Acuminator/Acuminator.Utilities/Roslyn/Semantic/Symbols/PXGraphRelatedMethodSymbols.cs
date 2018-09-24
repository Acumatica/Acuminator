using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXGraphRelatedMethodSymbols
    {
        private const string CreateInstanceMethodsName = "CreateInstance";
        private const string InstanceCreatedEventsAddHandlerName = "AddHandler";

        public ImmutableArray<IMethodSymbol> CreateInstance { get; }
        public IMethodSymbol InstanceCreatedEventsAddHandler { get; }

        internal PXGraphRelatedMethodSymbols(PXContext pxContext)
        {
	        CreateInstance = pxContext.PXGraphType.GetMethods(CreateInstanceMethodsName);

            InstanceCreatedEventsAddHandler = pxContext.InstanceCreatedEvents
	            .GetMethods(InstanceCreatedEventsAddHandlerName)
	            .First();
        }
    }
}
