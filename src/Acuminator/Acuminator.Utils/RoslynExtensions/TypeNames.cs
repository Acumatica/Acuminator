using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using PX.Data;


namespace Acuminator.Utilities
{
	public static class TypeNames
	{
		public static readonly string IBqlTable = typeof(IBqlTable).Name;

		public static readonly string BqlCommand = typeof(BqlCommand).Name;

		public static readonly string IBqlField = typeof(IBqlField).Name;

		public static readonly string IBqlParameter = typeof(IBqlParameter).Name;

		public static readonly string IBqlJoin = nameof(PX.Data.IBqlJoin);

		public static readonly string IBqlOrderBy = typeof(IBqlOrderBy).Name;

		public static readonly string IBqlAggregate = typeof(IBqlAggregate).Name;

		public static readonly string IBqlFunction = typeof(IBqlFunction).Name;

		public static readonly string IBqlSortColumn = typeof(IBqlSortColumn).Name;

		public static readonly string IBqlComparison = typeof(IBqlComparison).Name;

		public static readonly string IBqlCreator = typeof(IBqlCreator).Name;

		public static readonly string IBqlPredicateChain = typeof(IBqlPredicateChain).Name;

		public static readonly string IBqlOn = typeof(IBqlOn).Name;

		public static readonly string PXSelectBaseType = typeof(PXSelectBase).Name;

		public static readonly string Constant = typeof(Constant).Name;

		public static readonly string ConstantGeneric = typeof(Constant).Name + "`1";

		public static readonly string PXCacheExtension = typeof(PXCacheExtension).Name;

		public static readonly string PXCacheExtensionGeneric = typeof(PXCacheExtension).Name + "`1";

		public static readonly string PXGraph = typeof(PXGraph).Name;

		public static readonly string PXGraphGeneric = typeof(PXGraph<>).Name;

		public static readonly string PXGraphExtension = typeof(PXGraphExtension).Name;

		public static readonly string PXAction = typeof(PXAction).Name;

		public static readonly string PXActionGeneric = typeof(PXAction).Name + "`1";

		public static Dictionary<string, PXCodeType> TypeNamesToCodeTypesForIdentifier { get; } =
			new Dictionary<string, PXCodeType>
			{
				[IBqlTable] = PXCodeType.Dac,
				[IBqlField] = PXCodeType.DacField,
				[PXCacheExtension] = PXCodeType.DacExtension,
				[IBqlParameter] = PXCodeType.BqlParameter,
				[Constant] = PXCodeType.BQLConstantEnding,
				[PXSelectBaseType] = PXCodeType.BqlCommand,
				[BqlCommand] = PXCodeType.BqlCommand,
				[IBqlCreator] = PXCodeType.BqlOperator,
				[IBqlJoin] = PXCodeType.BqlOperator,
				[PXGraph] = PXCodeType.PXGraph
			};


		public static Dictionary<string, PXCodeType> TypeNamesToCodeTypesForGenericName { get; } =
			new Dictionary<string, PXCodeType>
			{
				[PXSelectBaseType] = PXCodeType.BqlCommand,
				[BqlCommand] = PXCodeType.BqlCommand,
				[IBqlParameter] = PXCodeType.BqlParameter,
				[IBqlCreator] = PXCodeType.BqlOperator,
				[IBqlJoin] = PXCodeType.BqlOperator,
				[PXAction] = PXCodeType.PXAction
			};

		public static HashSet<string> NotColoredTypes = new HashSet<string>
		{
			BqlCommand,
			PXCacheExtension,
			PXCacheExtensionGeneric,
			Constant,
			ConstantGeneric,
			PXGraph,
			PXGraphGeneric
		};
	}
}
