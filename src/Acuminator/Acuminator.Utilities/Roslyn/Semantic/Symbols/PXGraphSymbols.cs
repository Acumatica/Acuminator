#nullable enable

using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXGraphSymbols : SymbolsSetForTypeBase
	{
		public class InstanceCreatedEventsSymbols : SymbolsSetForTypeBase
	    {
			public IMethodSymbol? AddHandler { get; }

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

		public IMethodSymbol? InitCacheMapping => Type?.GetMethods(DelegateNames.InitCacheMapping)
													   .FirstOrDefault(method => method.ReturnsVoid && method.Parameters.Length == 1);

		public IMethodSymbol? Configure { get; }

		internal PXGraphSymbols(PXContext pxContext) : base(pxContext.Compilation, TypeFullNames.PXGraph)
        {
			Type.ThrowOnNull();

			GenericTypeGraph = Compilation.GetTypeByMetadataName(TypeFullNames.PXGraph1);
			GenericTypeGraphDac = Compilation.GetTypeByMetadataName(TypeFullNames.PXGraph2);
			GenericTypeGraphDacField = Compilation.GetTypeByMetadataName(TypeFullNames.PXGraph3);

			CreateInstance = Type.GetMethods(DelegateNames.CreateInstance).ToImmutableArray();
			InstanceCreatedEvents = new InstanceCreatedEventsSymbols(Compilation);

			Configure = Type.GetConfigureMethodFromBaseGraphOrGraphExtension(pxContext);
		}
    }
}
