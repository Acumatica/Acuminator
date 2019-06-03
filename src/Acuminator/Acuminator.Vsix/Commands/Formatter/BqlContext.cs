using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Vsix.Formatter
{
	class BqlContext
	{
		private readonly Compilation _compilation;

		public BqlContext(Compilation compilation)
		{
			_compilation = compilation;
		}

		public INamedTypeSymbol SelectBase => _compilation.GetTypeByMetadataName(TypeFullNames.SelectBase5);
		public INamedTypeSymbol SearchBase => _compilation.GetTypeByMetadataName(TypeFullNames.SearchBase5);
		public INamedTypeSymbol PXSelectBase => _compilation.GetTypeByMetadataName(TypeFullNames.PXSelectBase);

		public INamedTypeSymbol Where2 => _compilation.GetTypeByMetadataName(TypeFullNames.Where2);
		public INamedTypeSymbol And2 => _compilation.GetTypeByMetadataName(TypeFullNames.And2);
		public INamedTypeSymbol Or2 => _compilation.GetTypeByMetadataName(TypeFullNames.Or2);
		public INamedTypeSymbol Aggregate => _compilation.GetTypeByMetadataName(TypeFullNames.Aggregate);
		public INamedTypeSymbol GroupByBase => _compilation.GetTypeByMetadataName(TypeFullNames.GroupByBase2);
		public INamedTypeSymbol BqlPredicateBinaryBase => _compilation.GetTypeByMetadataName(TypeFullNames.BqlPredicateBinaryBase2);

		public INamedTypeSymbol IBqlCreator => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlCreator);
		public INamedTypeSymbol IBqlSelect => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlSelect);
		public INamedTypeSymbol IBqlSearch => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlSearch);
		public INamedTypeSymbol IBqlJoin => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlJoin);
		public INamedTypeSymbol IBqlOn => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlOn);
		public INamedTypeSymbol IBqlWhere => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlWhere);
		public INamedTypeSymbol IBqlOrderBy => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlOrderBy);
		public INamedTypeSymbol IBqlSortColumn => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlSortColumn);
		public INamedTypeSymbol IBqlFunction => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlFunction);
		public INamedTypeSymbol IBqlPredicateChain => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlPredicateChain);
		public INamedTypeSymbol IBqlTable => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlTable);


	}
}
