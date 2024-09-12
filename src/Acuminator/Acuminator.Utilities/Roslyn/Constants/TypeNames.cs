﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Constants
{
	public static class TypeNames
	{
		/// <summary>
		/// DAC referential integrity related constants.
		/// </summary>
		public static class ReferentialIntegrity
		{
			/// <summary>
			/// The mandatory name of the primary key class in DAC.
			/// </summary>
			public const string PrimaryKeyClassName = "PK";

			/// <summary>
			/// The mandatory name of the unique key class in DAC.
			/// </summary>
			public const string UniqueKeyClassName = "UK";

			/// <summary>
			/// The mandatory name of the foreign key class in DAC.
			/// </summary>
			public const string ForeignKeyClassName = "FK";

			public const string IPrimaryKeyOfName = "IPrimaryKeyOf";
			public const string IForeignKeyToName = "IForeignKeyTo";

			public const string PrimaryKeyOfName = "PrimaryKeyOf";
			public const string ForeignKeyOfName = "ForeignKeyOf";
			public const string AsSimpleKeyName = "AsSimpleKey";
			public const string CompositeKey = "CompositeKey";

			public const string By_TypeName = "By";
			public const string WithTablesOf_TypeName = "WithTablesOf";
			public const string IsRelatedTo = "IsRelatedTo";
			public const string Field = "Field";
		}

		/// <summary>
		/// DAC BQL fields related constants.
		/// </summary>
		public static class BqlField
		{
			public const string IBqlField = "IBqlField";
			public const string Field	  = "Field";
			public const string BqlType   = "BqlType";
		}

		/// <summary>
		/// C# predefined types.
		/// </summary>
		public static class CSharpPredefinedTypes
		{
			public const string Object 	= "object";
			public const string Dynamic = "dynamic";
			public const string Void 	= "void";
			public const string Boolean = "bool";
			public const string Char 	= "char";
			public const string SByte 	= "sbyte";
			public const string Byte 	= "byte";
			public const string Int16 	= "short";
			public const string UInt16 	= "ushort" ;
			public const string Int32 	= "int";
			public const string UInt32 	= "uint";
			public const string Int64 	= "long";
			public const string UInt64 	= "ulong";
			public const string Decimal = "decimal";
			public const string Single 	= "float";
			public const string Double 	= "double";
			public const string String 	= "string";
			public const string NInt 	= "nint";
			public const string NUInt 	= "nuint";

			public static ReadOnlyHashSet<string> All { get; } = 
				new ReadOnlyHashSet<string>
				([
					Object,
					Dynamic,
					Void,
					Boolean,
					Char,
					SByte,
					Byte,
					Int16,
					UInt16,
					Int32,
					UInt32,
					Int64,
					UInt64,
					Decimal,
					Single,
					Double,
					String,
					NInt,
					NUInt
				],
				StringComparer.OrdinalIgnoreCase);
		}

		public const string PXView = "PXView";

		public const string PXAdapter = "PXAdapter";

		public const string IBqlTable = "IBqlTable";
		public const string PXBqlTable = "PXBqlTable";

		public const string BqlCommand = "BqlCommand";
		public const string FbqlCommand = "FbqlCommand";
		
		public const string IBqlParameter = "IBqlParameter";
		public const string IBqlJoin = "IBqlJoin";
		public const string IBqlOrderBy = "IBqlOrderBy";
		public const string IBqlAggregate = "IBqlAggregate";
		public const string IBqlFunction = "IBqlFunction";
		public const string IBqlSortColumn = "IBqlSortColumn";
		public const string IBqlComparison = "IBqlComparison";
		public const string IBqlCreator = "IBqlCreator";
		public const string IBqlOperand = "IBqlOperand";
		public const string IBqlPredicateChain = "IBqlPredicateChain";
		public const string IBqlOn = "IBqlOn";
		public const string IBqlSet = "IBqlSet";

		public const string PXSelectBaseType = "PXSelectBase";

		public const string Constant = "Constant";
		public const string ConstantGeneric = "Constant`1";

		public const string PXCacheExtension = "PXCacheExtension";
		public const string PXCacheExtensionGeneric = "PXCacheExtension`1";

		public const string PXGraph = "PXGraph";

		public const string PXGraphGeneric = "PXGraph`1";
		public const string PXGraphExtension = "PXGraphExtension";

		public const string PXAction = "PXAction";
		public const string PXActionGeneric = "PXAction`1";

		public const string PXUpdate = "PXUpdate";
		public const string PXUpdateJoin = "PXUpdateJoin";
		public const string PXUpdateGroupBy = "PXUpdateGroupBy";
		public const string PXUpdateJoinGroupBy = "PXUpdateJoinGroupBy";

		public const string PXSelectReadonly = "PXSelectReadonly";
		public const string PXSelectReadonly2 = "PXSelectReadonly2";
		public const string PXSelectReadonly3 = "PXSelectReadonly3";

		public const string FbqlSelect = "FbqlSelect";

		public const string FullJoin = "FullJoin";
		public const string RightJoin = "RightJoin";
		public const string LeftJoin = "LeftJoin";
		public const string InnerJoin = "InnerJoin";

		public const string PXUnboundDefault = "PXUnboundDefault";
		public const string PXPersistingCheck = "PXPersistingCheck";
		public const string PXDefault = "PXDefault";
		public const string PersistingCheck = "PersistingCheck";
		public const string PersistingCheckNothing = "Nothing";


		public static ImmutableDictionary<string, PXCodeType> TypeNamesToCodeTypesForIdentifier { get; } =
			new Dictionary<string, PXCodeType>
				{
					[IBqlTable] 		 = PXCodeType.Dac,
					[BqlField.IBqlField] = PXCodeType.DacField,
					[PXCacheExtension] 	 = PXCodeType.DacExtension,
					[IBqlParameter] 	 = PXCodeType.BqlParameter,
					[Constant] 			 = PXCodeType.BQLConstantEnding,

					[PXSelectBaseType] 	  = PXCodeType.BqlCommand,
					[BqlCommand] 		  = PXCodeType.BqlCommand,
					[PXUpdate] 			  = PXCodeType.BqlCommand,
					[PXUpdateJoin] 		  = PXCodeType.BqlCommand,
					[PXUpdateGroupBy] 	  = PXCodeType.BqlCommand,
					[PXUpdateJoinGroupBy] = PXCodeType.BqlCommand,

					[IBqlCreator] = PXCodeType.BqlOperator,
					[IBqlJoin] 	  = PXCodeType.BqlOperator,
					[IBqlSet] 	  = PXCodeType.BqlOperator,

					[FullJoin] 	= PXCodeType.BqlOperator,
					[RightJoin] = PXCodeType.BqlOperator,
					[LeftJoin] 	= PXCodeType.BqlOperator,
					[InnerJoin] = PXCodeType.BqlOperator,

					[PXGraph] = PXCodeType.PXGraph
				}
				.ToImmutableDictionary();

		public static ImmutableDictionary<string, PXCodeType> TypeNamesToCodeTypesForGenericName { get; } =
			new Dictionary<string, PXCodeType>
				{
					[PXSelectBaseType] 	  = PXCodeType.BqlCommand,
					[BqlCommand] 		  = PXCodeType.BqlCommand,
					[PXUpdate] 			  = PXCodeType.BqlCommand,
					[PXUpdateJoin] 		  = PXCodeType.BqlCommand,
					[PXUpdateGroupBy] 	  = PXCodeType.BqlCommand,
					[PXUpdateJoinGroupBy] = PXCodeType.BqlCommand,

					[IBqlParameter] = PXCodeType.BqlParameter,
					[IBqlCreator] 	= PXCodeType.BqlOperator,
					[IBqlJoin] 		= PXCodeType.BqlOperator,
					[IBqlSet] 		= PXCodeType.BqlOperator,

					[FullJoin] 	= PXCodeType.BqlOperator,
					[RightJoin] = PXCodeType.BqlOperator,
					[LeftJoin] 	= PXCodeType.BqlOperator,
					[InnerJoin] = PXCodeType.BqlOperator,

					[PXAction] = PXCodeType.PXAction,
				}
				.ToImmutableDictionary();

		public static ImmutableHashSet<string> PXUpdateBqlTypes { get; } = new string[]
		{
			PXUpdate,
			PXUpdateJoin,
			PXUpdateGroupBy,
			PXUpdateJoinGroupBy
		}.ToImmutableHashSet();

		public static ImmutableHashSet<string> NotColoredTypes { get; } = new string[]
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

		public static ImmutableHashSet<string> FBqlJoins { get; } = new string[]
		{
			FullJoin,
			RightJoin,
			LeftJoin,
			InnerJoin,
		}.ToImmutableHashSet();
	}
}