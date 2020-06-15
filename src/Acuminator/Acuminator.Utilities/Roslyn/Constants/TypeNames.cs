using System.Collections.Generic;
using System.Collections.Immutable;

namespace Acuminator.Utilities.Roslyn.Constants
{
	public static class TypeNames
	{
		/// <summary>
		/// The mandatory name of the primary key class in DAC.
		/// </summary>
		public static readonly string PrimaryKeyClassName = "PK";

		/// <summary>
		/// The mandatory name of the foreign key class in DAC.
		/// </summary>
		public static readonly string ForeignKeyClassName = "FK";

		public static readonly string ForeignKeyOfMetadataName = "ForeignKeyOf`1";

		public static readonly string PXView = "PXView";

		public static readonly string IBqlTable = "IBqlTable";

		public static readonly string BqlCommand = "BqlCommand";
		public static readonly string FbqlCommand = "FbqlCommand";
		public static readonly string IBqlField = "IBqlField";
		public static readonly string IBqlParameter = "IBqlParameter";
		public static readonly string IBqlJoin = "IBqlJoin";
		public static readonly string IBqlOrderBy = "IBqlOrderBy";
		public static readonly string IBqlAggregate = "IBqlAggregate";
		public static readonly string IBqlFunction = "IBqlFunction";
		public static readonly string IBqlSortColumn = "IBqlSortColumn";
		public static readonly string IBqlComparison = "IBqlComparison";
		public static readonly string IBqlCreator = "IBqlCreator";
		public static readonly string IBqlOperand = "IBqlOperand";
		public static readonly string IBqlPredicateChain = "IBqlPredicateChain";
		public static readonly string IBqlOn = "IBqlOn";
		public static readonly string IBqlSet = "IBqlSet";

		public static readonly string PXSelectBaseType = "PXSelectBase";

		public static readonly string Constant = "Constant";
		public static readonly string ConstantGeneric = "Constant`1";

		public static readonly string PXCacheExtension = "PXCacheExtension";
		public static readonly string PXCacheExtensionGeneric = "PXCacheExtension`1";

		public static readonly string PXGraph = "PXGraph";

		public static readonly string PXGraphGeneric = "PXGraph`1";
		public static readonly string PXGraphExtension = "PXGraphExtension";

		public static readonly string PXAction = "PXAction";
		public static readonly string PXActionGeneric = "PXAction`1";

		public static readonly string PXUpdate = "PXUpdate";
		public static readonly string PXUpdateJoin = "PXUpdateJoin";
		public static readonly string PXUpdateGroupBy = "PXUpdateGroupBy";
		public static readonly string PXUpdateJoinGroupBy = "PXUpdateJoinGroupBy";

		public static readonly string PXSelectReadonly = "PXSelectReadonly";
		public static readonly string PXSelectReadonly2 = "PXSelectReadonly2";
		public static readonly string PXSelectReadonly3 = "PXSelectReadonly3";

		public static readonly string FbqlSelect = "FbqlSelect";

		public static readonly string FullJoin = "FullJoin";
		public static readonly string RightJoin = "RightJoin";
		public static readonly string LeftJoin = "LeftJoin";
		public static readonly string InnerJoin = "InnerJoin";

		public static readonly string PXUnboundDefault = "PXUnboundDefault";
		public static readonly string PXPersistingCheck = "PXPersistingCheck";
		public static readonly string PXDefault = "PXDefault";
		public static readonly string PersistingCheck = "PersistingCheck";
		public static readonly string PersistingCheckNothing = "Nothing";



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
		}.ToImmutableHashSet();

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
		}.ToImmutableHashSet();

		public static ImmutableHashSet<string> ReadOnlySelects { get; } = new string[]
		{
			PXSelectReadonly,
			PXSelectReadonly2,
			PXSelectReadonly3,
		}.ToImmutableHashSet();

		public static ImmutableHashSet<string> FBqlJoins = new string[]
		{
			FullJoin,
			RightJoin,
			LeftJoin,
			InnerJoin,
		}.ToImmutableHashSet();
	}
}