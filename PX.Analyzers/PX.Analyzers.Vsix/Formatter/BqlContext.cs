using Microsoft.CodeAnalysis;
using PX.Data;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlContext
	{
		private readonly Compilation _compilation;

		public BqlContext(Compilation compilation)
		{
			_compilation = compilation;
		}

		public INamedTypeSymbol SelectBase => _compilation.GetTypeByMetadataName(typeof(SelectBase<,,,,>).FullName);
		public INamedTypeSymbol SearchBase => _compilation.GetTypeByMetadataName(typeof(SearchBase<,,,,>).FullName);
		public INamedTypeSymbol PXSelectBase => _compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);

		public INamedTypeSymbol Where2 => _compilation.GetTypeByMetadataName(typeof(Where2<,>).FullName);

		public INamedTypeSymbol IBqlSelect => _compilation.GetTypeByMetadataName(typeof(IBqlSelect).FullName);
		public INamedTypeSymbol IBqlSearch => _compilation.GetTypeByMetadataName(typeof(IBqlSearch).FullName);
		public INamedTypeSymbol IBqlWhere => _compilation.GetTypeByMetadataName(typeof(IBqlWhere).FullName);

		public INamedTypeSymbol IBqlTable => _compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);
	}
}
