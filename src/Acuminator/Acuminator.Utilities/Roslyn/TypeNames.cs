using System.Collections.Generic;
using System.Collections.Immutable;
using PX.Data;

namespace Acuminator.Utilities.Roslyn
{
	public static class TypeNames
	{
		public static readonly string IBqlTable = typeof(IBqlTable).Name;

		public static readonly string BqlCommand = typeof(BqlCommand).Name;
		public const string FbqlCommand = nameof(FbqlCommand);

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
		public static readonly string IBqlSet = typeof(IBqlSet).Name;

		public static readonly string PXSelectBaseType = typeof(PXSelectBase).Name;

		public static readonly string Constant = typeof(Constant).Name;
		public static readonly string ConstantGeneric = typeof(Constant).Name + "`1";

		public static readonly string PXCacheExtension = typeof(PXCacheExtension).Name;
		public static readonly string PXCacheExtensionGeneric = typeof(PXCacheExtension).Name + "`1";

		public static readonly string PXGraph = "PXGraph";
		public static readonly string PXGraphGeneric = typeof(PXGraph<>).Name;
		public static readonly string PXGraphExtension = typeof(PXGraphExtension).Name;

		public static readonly string PXAction = typeof(PXAction).Name;
		public static readonly string PXActionGeneric = typeof(PXAction).Name + "`1";

		public const string PXUpdate = nameof(PXUpdate);
		public const string PXUpdateJoin = nameof(PXUpdateJoin);
		public const string PXUpdateGroupBy = nameof(PXUpdateGroupBy);
		public const string PXUpdateJoinGroupBy = nameof(PXUpdateJoinGroupBy);

		public const string PXSelectReadonly = nameof(PXSelectReadonly<DummyDac>);
		public const string PXSelectReadonly2 = nameof(PXSelectReadonly2);
		public const string PXSelectReadonly3 = nameof(PXSelectReadonly3);

		public const string PXSelectBase_Acumatica2018R2 = "PX.Data.PXSelectBase`2";
		public const string IViewConfig_Acumatica2018R2 = "PX.Data.PXSelectBase`2+IViewConfig";
		public const string PXGraphTypeName = "PX.Data.PXGraph";

		public const string FbqlSelect = nameof(FbqlSelect);

		public const string FullJoin = nameof(FullJoin);
		public const string RightJoin = nameof(RightJoin);
		public const string LeftJoin = nameof(LeftJoin);
		public const string InnerJoin = nameof(InnerJoin);
		

		public static ImmutableDictionary<string, PXCodeType> TypeNamesToCodeTypesForIdentifier { get; } =
			new Dictionary<string, PXCodeType>
			{
				[IBqlTable] = PXCodeType.Dac,
				[IBqlField] = PXCodeType.DacField,
				[PXCacheExtension] = PXCodeType.DacExtension,
				[IBqlParameter] = PXCodeType.BqlParameter,
				[Constant] = PXCodeType.BQLConstantEnding,

				[PXSelectBaseType] = PXCodeType.BqlCommand,
				[BqlCommand] = PXCodeType.BqlCommand,
				[PXUpdate] = PXCodeType.BqlCommand,
				[PXUpdateJoin] = PXCodeType.BqlCommand,
				[PXUpdateGroupBy] = PXCodeType.BqlCommand,
				[PXUpdateJoinGroupBy] = PXCodeType.BqlCommand,

				[IBqlCreator] = PXCodeType.BqlOperator,
				[IBqlJoin] = PXCodeType.BqlOperator,
				[IBqlSet] = PXCodeType.BqlOperator,

				[FullJoin] = PXCodeType.BqlOperator,
				[RightJoin] = PXCodeType.BqlOperator,
				[LeftJoin] = PXCodeType.BqlOperator,
				[InnerJoin] = PXCodeType.BqlOperator,

				[PXGraph] = PXCodeType.PXGraph
			}
			.ToImmutableDictionary();

		public static ImmutableDictionary<string, PXCodeType> TypeNamesToCodeTypesForGenericName { get; } =
			new Dictionary<string, PXCodeType>
			{
				[PXSelectBaseType] = PXCodeType.BqlCommand,
				[BqlCommand] = PXCodeType.BqlCommand,
				[PXUpdate] = PXCodeType.BqlCommand,
				[PXUpdateJoin] = PXCodeType.BqlCommand,
				[PXUpdateGroupBy] = PXCodeType.BqlCommand,
				[PXUpdateJoinGroupBy] = PXCodeType.BqlCommand,

				[IBqlParameter] = PXCodeType.BqlParameter,
				[IBqlCreator] = PXCodeType.BqlOperator,
				[IBqlJoin] = PXCodeType.BqlOperator,
				[IBqlSet] = PXCodeType.BqlOperator,
				
				[FullJoin] = PXCodeType.BqlOperator,
				[RightJoin] = PXCodeType.BqlOperator,
				[LeftJoin] = PXCodeType.BqlOperator,
				[InnerJoin] = PXCodeType.BqlOperator,

				[PXAction] = PXCodeType.PXAction,
			}
			.ToImmutableDictionary();

		public static ImmutableHashSet<string> PXUpdateBqlTypes = new string[]
		{
			PXUpdate,
			PXUpdateJoin,
			PXUpdateGroupBy,
			PXUpdateJoinGroupBy
		}
		.ToImmutableHashSet();

		public static ImmutableHashSet<string> NotColoredTypes = new string[]
		{
			BqlCommand,
			FbqlCommand,
			PXCacheExtension,
			PXCacheExtensionGeneric,
			Constant,
			ConstantGeneric,
			PXGraph,
			PXGraphGeneric
		}
		.ToImmutableHashSet();

		public static ImmutableHashSet<string> ReadOnlySelects { get; } = new string[]
		{
			PXSelectReadonly,
			PXSelectReadonly2,
			PXSelectReadonly3,
		}
		.ToImmutableHashSet();

		public static ImmutableHashSet<string> FBqlJoins = new string[]
		{
			FullJoin,
			RightJoin,
			LeftJoin,
			InnerJoin,
		}
		.ToImmutableHashSet();
	}
}
