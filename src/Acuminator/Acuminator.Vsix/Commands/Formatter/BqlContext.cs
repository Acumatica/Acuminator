using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Vsix.Formatter
{
	class BqlContext
	{
		private readonly Compilation _compilation;

		public BqlContext(Compilation compilation)
		{
			_compilation = compilation;
		}

		public INamedTypeSymbol SelectBase => _compilation.GetTypeByMetadataName(Types.SelectBase5);
		public INamedTypeSymbol SearchBase => _compilation.GetTypeByMetadataName(Types.SearchBase5);
		public INamedTypeSymbol PXSelectBase => _compilation.GetTypeByMetadataName(Types.PXSelectBase);

		public INamedTypeSymbol Where2 => _compilation.GetTypeByMetadataName(Types.Where2);
		public INamedTypeSymbol And2 => _compilation.GetTypeByMetadataName(Types.And2);
		public INamedTypeSymbol Or2 => _compilation.GetTypeByMetadataName(Types.Or2);
		public INamedTypeSymbol Aggregate => _compilation.GetTypeByMetadataName(Types.Aggregate);
		public INamedTypeSymbol GroupByBase => _compilation.GetTypeByMetadataName(Types.GroupByBase2);
		public INamedTypeSymbol BqlPredicateBinaryBase => _compilation.GetTypeByMetadataName(Types.BqlPredicateBinaryBase2);

		public INamedTypeSymbol IBqlCreator => _compilation.GetTypeByMetadataName(Types.IBqlCreator);
		public INamedTypeSymbol IBqlSelect => _compilation.GetTypeByMetadataName(Types.IBqlSelect);
		public INamedTypeSymbol IBqlSearch => _compilation.GetTypeByMetadataName(Types.IBqlSearch);
		public INamedTypeSymbol IBqlJoin => _compilation.GetTypeByMetadataName(Types.IBqlJoin);
		public INamedTypeSymbol IBqlOn => _compilation.GetTypeByMetadataName(Types.IBqlOn);
		public INamedTypeSymbol IBqlWhere => _compilation.GetTypeByMetadataName(Types.IBqlWhere);
		public INamedTypeSymbol IBqlOrderBy => _compilation.GetTypeByMetadataName(Types.IBqlOrderBy);
		public INamedTypeSymbol IBqlSortColumn => _compilation.GetTypeByMetadataName(Types.IBqlSortColumn);
		public INamedTypeSymbol IBqlFunction => _compilation.GetTypeByMetadataName(Types.IBqlFunction);
		public INamedTypeSymbol IBqlPredicateChain => _compilation.GetTypeByMetadataName(Types.IBqlPredicateChain);
		public INamedTypeSymbol IBqlTable => _compilation.GetTypeByMetadataName(Types.IBqlTable);


	}
}
