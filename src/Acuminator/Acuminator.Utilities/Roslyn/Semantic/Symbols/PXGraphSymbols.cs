using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXGraphSymbols
    {
	    public class InstanceCreatedEventsSymbols
	    {
		    public INamedTypeSymbol Type { get; }
			public IMethodSymbol AddHandler { get; }

		    internal InstanceCreatedEventsSymbols(Compilation compilation)
		    {
			    Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXGraph.InstanceCreatedEvents).FullName);
				AddHandler = Type.GetMethods(nameof(PX.Data.PXGraph.InstanceCreatedEvents.AddHandler)).First();
		    }
	    }

        public INamedTypeSymbol Type { get; }

        public ImmutableArray<IMethodSymbol> CreateInstance { get; }
	    public InstanceCreatedEventsSymbols InstanceCreatedEvents { get; }

        internal PXGraphSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(typeof(PX.Data.PXGraph).FullName);

	        CreateInstance = Type.GetMethods(nameof(PX.Data.PXGraph.CreateInstance));
			InstanceCreatedEvents = new InstanceCreatedEventsSymbols(compilation);
		        
        }
    }
}
