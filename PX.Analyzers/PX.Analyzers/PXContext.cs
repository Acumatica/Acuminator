using Microsoft.CodeAnalysis;
using PX.Data;

namespace PX.Analyzers
{
	public class PXContext
	{
		internal PXContext(Compilation compilation)
		{
			Compilation = compilation;
		}

		public Compilation Compilation { get; }

		public INamedTypeSymbol PXGraphType => Compilation.GetTypeByMetadataName(typeof(PXGraph).FullName);
		public INamedTypeSymbol PXActionType => Compilation.GetTypeByMetadataName(typeof(PXAction).FullName);
		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(typeof(PXAdapter).FullName);
	}
}