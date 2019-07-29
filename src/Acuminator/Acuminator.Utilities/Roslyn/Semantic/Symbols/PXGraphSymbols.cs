using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXGraphSymbols : SymbolsSetForTypeBase
	{
		public class InstanceCreatedEventsSymbols : SymbolsSetForTypeBase
	    {
			public IMethodSymbol AddHandler { get; }

		    internal InstanceCreatedEventsSymbols(Compilation compilation) : base(compilation, DelegateNames.InstanceCreatedEvents)
		    {
				AddHandler = Type?.GetMethods(DelegateNames.InstanceCreatedEventsAddHandler).First();
		    }
	    }

		public INamedTypeSymbol GenericTypeGraph { get; }
		public INamedTypeSymbol GenericTypeGraphDac { get; }
		public INamedTypeSymbol GenericTypeGraphDacField { get; }

		public ImmutableArray<IMethodSymbol> CreateInstance { get; }

	    public InstanceCreatedEventsSymbols InstanceCreatedEvents { get; }

		public IMethodSymbol InitCacheMapping => Type.GetMembers(DelegateNames.InitCacheMapping)
													 .OfType<IMethodSymbol>()
													 .FirstOrDefault(method => method.ReturnsVoid && method.Parameters.Length == 1);

		internal PXGraphSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXGraph)
        {
			GenericTypeGraph = compilation.GetTypeByMetadataName(TypeFullNames.PXGraph1);
			GenericTypeGraphDac = compilation.GetTypeByMetadataName(TypeFullNames.PXGraph2);
			GenericTypeGraphDacField = compilation.GetTypeByMetadataName(TypeFullNames.PXGraph3);

			CreateInstance = Type.GetMethods(DelegateNames.CreateInstance);
			InstanceCreatedEvents = new InstanceCreatedEventsSymbols(compilation);
        }
    }
}
