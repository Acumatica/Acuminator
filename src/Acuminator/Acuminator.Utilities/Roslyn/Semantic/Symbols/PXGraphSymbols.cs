using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;


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
			    Type = compilation.GetTypeByMetadataName(Types.PXGraphNames.InstanceCreatedEvents);
				AddHandler = Type.GetMethods(Types.PXGraphNames.InstanceCreatedEventsAddHabdler).First();
		    }
	    }

        public INamedTypeSymbol Type { get; }
		public INamedTypeSymbol GenericTypeGraph { get; }
		public INamedTypeSymbol GenericTypeGraphDac { get; }
		public INamedTypeSymbol GenericTypeGraphDacField { get; }

		public ImmutableArray<IMethodSymbol> CreateInstance { get; }

	    public InstanceCreatedEventsSymbols InstanceCreatedEvents { get; }

		public IMethodSymbol InitCacheMapping => Type.GetMembers(Types.PXGraphNames.InitCacheMapping)
													 .OfType<IMethodSymbol>()
													 .FirstOrDefault(method => method.ReturnsVoid && method.Parameters.Length == 1);

		internal PXGraphSymbols(Compilation compilation)
        {
            Type = compilation.GetTypeByMetadataName(Types.PXGraph);
			GenericTypeGraph = compilation.GetTypeByMetadataName(Types.PXGraph1);
			GenericTypeGraphDac = compilation.GetTypeByMetadataName(Types.PXGraph2);
			GenericTypeGraphDacField = compilation.GetTypeByMetadataName(Types.PXGraph3);

			CreateInstance = Type.GetMethods(Types.PXGraphNames.CreateInstance);
			InstanceCreatedEvents = new InstanceCreatedEventsSymbols(compilation);
        }
    }
}
