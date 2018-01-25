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
		public INamedTypeSymbol Aggregate => _compilation.GetTypeByMetadataName(typeof(Aggregate<>).FullName);
		public INamedTypeSymbol GroupByBase => _compilation.GetTypeByMetadataName(typeof(GroupByBase<,>).FullName);
		public INamedTypeSymbol BqlPredicateBinaryBase => _compilation.GetTypeByMetadataName(typeof(BqlPredicateBinaryBase<,>).FullName);

		public INamedTypeSymbol IBqlCreator => _compilation.GetTypeByMetadataName(typeof(IBqlCreator).FullName);
		public INamedTypeSymbol IBqlSelect => _compilation.GetTypeByMetadataName(typeof(IBqlSelect).FullName);
		public INamedTypeSymbol IBqlSearch => _compilation.GetTypeByMetadataName(typeof(IBqlSearch).FullName);
		public INamedTypeSymbol IBqlJoin => _compilation.GetTypeByMetadataName(typeof(IBqlJoin).FullName);
		public INamedTypeSymbol IBqlOn => _compilation.GetTypeByMetadataName(typeof(IBqlOn).FullName);
		public INamedTypeSymbol IBqlWhere => _compilation.GetTypeByMetadataName(typeof(IBqlWhere).FullName);
		public INamedTypeSymbol IBqlOrderBy => _compilation.GetTypeByMetadataName(typeof(IBqlOrderBy).FullName);
		public INamedTypeSymbol IBqlSortColumn => _compilation.GetTypeByMetadataName(typeof(IBqlSortColumn).FullName);
		public INamedTypeSymbol IBqlFunction => _compilation.GetTypeByMetadataName(typeof(IBqlFunction).FullName);
		public INamedTypeSymbol IBqlPredicateChain => _compilation.GetTypeByMetadataName(typeof(IBqlPredicateChain).FullName);

		public INamedTypeSymbol IBqlTable => _compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);
	}
}
