using Microsoft.CodeAnalysis;

namespace PX.Analyzers
{
	public class PXContext
	{
		internal PXContext(Compilation compilation)
		{
			Compilation = compilation;
		}

		public Compilation Compilation { get; }

		public INamedTypeSymbol PXGraphType => Compilation.GetTypeByMetadataName(Constants.Types.PXGraph);
		public INamedTypeSymbol PXActionType => Compilation.GetTypeByMetadataName(Constants.Types.PXAction);
		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(Constants.Types.PXAdapter);
	}
}