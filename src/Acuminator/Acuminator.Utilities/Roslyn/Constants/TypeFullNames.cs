namespace Acuminator.Utilities.Roslyn.Constants
{
	public class TypeFullNames
	{
		#region FieldAttributeSymbols
		/// <summary>
		/// The PXDBPackedIntegerArrayAttribute type full name in Acumatica 2018R2. Doesn't exist in previous versions.
		/// </summary>
		internal static readonly string PXDBPackedIntegerArrayAttributeFullName_Acumatica2018R2 = "PX.Data.PXDBPackedIntegerArrayAttribute";

		internal static readonly string PeriodIDAttribute = "PX.Objects.GL.PeriodIDAttribute";
		internal static readonly string AcctSubAttribute = "PX.Objects.GL.AcctSubAttribute";
		internal static readonly string PXDBCalcedAttribute = "PX.Data.PXDBCalcedAttribute";
		internal static readonly string PXDBScalarAttribute = "PX.Data.PXDBScalarAttribute";
		internal static readonly string PXDBUserPasswordAttribute = "PX.Data.PXDBUserPasswordAttribute";
		internal static readonly string PXDBAttributeAttribute = "PX.Data.PXDBAttributeAttribute";
		internal static readonly string PXDBDataLengthAttribute = "PX.Data.PXDBDataLengthAttribute";
		internal static readonly string PXDBBinaryAttribute = "PX.Data.PXDBBinaryAttribute";
		internal static readonly string PXDBLongIdentityAttribute = "PX.Data.PXDBLongIdentityAttribute";
		internal static readonly string PXDBIdentityAttribute = "PX.Data.PXDBIdentityAttribute";
		internal static readonly string PXDBTimestampAttribute = "PX.Data.PXDBTimestampAttribute";
		internal static readonly string PXDBBoolAttribute = "PX.Data.PXDBBoolAttribute";
		internal static readonly string PXDBGuidAttribute = "PX.Data.PXDBGuidAttribute";
		internal static readonly string PXDBDateAttribute = "PX.Data.PXDBDateAttribute";
		internal static readonly string PXDBFloatAttribute = "PX.Data.PXDBFloatAttribute";
		internal static readonly string PXDBDoubleAttribute = "PX.Data.PXDBDoubleAttribute";

		internal static readonly string PXLongAttribute = "PX.Data.PXLongAttribute";
		internal static readonly string PXIntAttribute = "PX.Data.PXIntAttribute";
		internal static readonly string PXShortAttribute = "PX.Data.PXShortAttribute";
		internal static readonly string PXStringAttribute = "PX.Data.PXStringAttribute";
		internal static readonly string PXByteAttribute = "PX.Data.PXByteAttribute";
		internal static readonly string PXDecimalAttribute = "PX.Data.PXDecimalAttribute";
		internal static readonly string PXDoubleAttribute = "PX.Data.PXDoubleAttribute";
		internal static readonly string PXFloatAttribute = "PX.Data.PXFloatAttribute";
		internal static readonly string PXDateAttribute = "PX.Data.PXDateAttribute";
		internal static readonly string PXGuidAttribute = "PX.Data.PXGuidAttribute";
		internal static readonly string PXBoolAttribute = "PX.Data.PXBoolAttribute";
		internal static readonly string PXDBFieldAttribute = "PX.Data.PXDBFieldAttribute";

		internal static readonly string PXDBLongAttribute = "PX.Data.PXDBLongAttribute";
		internal static readonly string PXDBIntAttribute = "PX.Data.PXDBIntAttribute";
		internal static readonly string PXDBShortAttribute = "PX.Data.PXDBShortAttribute";
		internal static readonly string PXDBStringAttribute = "PX.Data.PXDBStringAttribute";
		internal static readonly string PXDBByteAttribute = "PX.Data.PXDBByteAttribute";
		internal static readonly string PXDBDecimalAttribute = "PX.Data.PXDBDecimalAttribute";

		#endregion

		#region PXUIFieldAttributeSymbols
		internal static readonly string PXUIFieldAttribute = "PX.Data.PXUIFieldAttribute";
		#endregion

		#region PXFilteredProcessingGraphRule
		internal static readonly string PXFilteredProcessing = "PX.Data.PXFilteredProcessing`2";
		#endregion

		#region ViewsWithoutPXViewNameAttributeGraphRule && PXViewNameAttributeViewRule
		internal static readonly string PXViewNameAttribute = "PX.Data.PXViewNameAttribute";
		#endregion

		#region PXContext
		internal static readonly string PXGraphExtension = "PX.Data.PXGraphExtension";
		internal static readonly string PXCacheExtension = "PX.Data.PXCacheExtension";
		internal static readonly string PXMappedCacheExtension = "PX.Data.PXMappedCacheExtension";
		internal static readonly string PXLongOperation = "PX.Data.PXLongOperation";
		internal static readonly string PXActionCollection = "PX.Data.PXActionCollection";
		internal static readonly string PXAdapter = "PX.Data.PXAdapter";

		internal static readonly string IBqlField = "PX.Data.IBqlField";
		internal static readonly string Constant = "PX.Data.Constant`1";
		internal static readonly string IPXResultset = "PX.Data.IPXResultset";
		internal static readonly string PXResult = "PX.Data.PXResult";
		internal static readonly string PXFieldState = "PX.Data.PXFieldState";
		internal static readonly string PXAttributeFamilyAttribute = "PX.Data.PXAttributeFamilyAttribute";
		internal static readonly string IPXLocalizableList = "PX.Data.IPXLocalizableList";
		internal static readonly string PXConnectionScope = "PX.Data.PXConnectionScope";

		internal static readonly string IImplementType = "PX.Common.IImplement`1";
		#endregion

		#region AttributeSymbol
		internal static readonly string PXImportAttribute = "PX.Data.PXImportAttribute";
		internal static readonly string PXHiddenAttribute = "PX.Data.PXHiddenAttribute";
		internal static readonly string PXCacheNameAttribute = "PX.Data.PXCacheNameAttribute";
		internal static readonly string PXCopyPasteHiddenViewAttribute = "PX.Data.PXCopyPasteHiddenViewAttribute";
		internal static readonly string PXOverrideAttribute = "PX.Data.PXOverrideAttribute";
		internal static readonly string PXEventSubscriberAttribute = "PX.Data.PXEventSubscriberAttribute";
		internal static readonly string PXAggregateAttribute = "PX.Data.PXAggregateAttribute";
		internal static readonly string PXDynamicAggregateAttribute = "PX.Data.PXDynamicAggregateAttribute";
		internal static readonly string PXDefaultAttribute = "PX.Data.PXDefaultAttribute";
		internal static readonly string PXUnboundDefaultAttribute = "PX.Data.PXUnboundDefaultAttribute";
		internal static readonly string PXButtonAttribute = "PX.Data.PXButtonAttribute";
		#endregion

		#region BqlDataTypeSymbols
		internal static readonly string BqlDataTypeType = "PX.Data.BQL.IBqlDataType";
		internal static readonly string BqlStringType = "PX.Data.BQL.IBqlString";
		internal static readonly string BqlGuidType = "PX.Data.BQL.IBqlGuid";
		internal static readonly string BqlDateTimeType = "PX.Data.BQL.IBqlDateTime";
		internal static readonly string BqlBoolType = "PX.Data.BQL.IBqlBool";
		internal static readonly string BqlByteType = "PX.Data.BQL.IBqlByte";
		internal static readonly string BqlShortType = "PX.Data.BQL.IBqlShort";
		internal static readonly string BqlIntType = "PX.Data.BQL.IBqlInt";
		internal static readonly string BqlLongType = "PX.Data.BQL.IBqlLong";
		internal static readonly string BqlFloatType = "PX.Data.BQL.IBqlFloat";
		internal static readonly string BqlDoubleType = "PX.Data.BQL.IBqlDouble";
		internal static readonly string BqlDecimalType = "PX.Data.BQL.IBqlDecimal";
		internal static readonly string BqlByteArrayType = "PX.Data.BQL.IBqlByteArray";
		#endregion

		#region BQLSymbols
		internal static readonly string CustomPredicate = "PX.Data.CustomPredicate";
		internal static readonly string AreSame2 = "PX.Data.AreSame`2";
		internal static readonly string AreDistinct2 = "PX.Data.AreDistinct`2";
		internal static readonly string Required1 = "PX.Data.Required`1";
		internal static readonly string Argument1 = "PX.Data.Argument`1";
		internal static readonly string Optional1 = "PX.Data.Optional`1";
		internal static readonly string Optional2 = "PX.Data.Optional2`1";
		internal static readonly string BqlCommand = "PX.Data.BqlCommand";
		internal static readonly string IBqlParameter = "PX.Data.IBqlParameter";
		internal static readonly string PXFilter1 = "PX.Data.PXFilter`1";
		internal static readonly string IPXNonUpdateable = "PX.Data.IPXNonUpdateable";
		internal static readonly string PXSetup1 = "PX.Data.PXSetup`1";
		internal static readonly string PXSetup2 = "PX.Data.PXSetup`2";
		internal static readonly string PXSetup3 = "PX.Data.PXSetup`3";
		internal static readonly string PXSetupSelect = "PX.Data.PXSetupSelect`1";
		internal static readonly string PXViewOfBasedOn = "PX.Data.BQL.Fluent.PXViewOf`1+BasedOn`1";
		internal static readonly string PXViewOf = "PX.Data.BQL.Fluent.PXViewOf`1";
		internal static readonly string FbqlCommand = "PX.Data.BQL.Fluent.FbqlCommand";
		#endregion

		#region ExceptionSymbols
		internal static readonly string PXException = "PX.Data.PXException";
		internal static readonly string PXBaseRedirectException = "PX.Data.PXBaseRedirectException";
		internal static readonly string PXSetupNotEnteredException = "PX.Data.PXSetupNotEnteredException";
		#endregion

		#region PXActionSymbol
		internal static readonly string PXAction = "PX.Data.PXAction";
		#endregion

		#region PXCacheSymbols
		internal static readonly string PXCache = "PX.Data.PXCache";
		#endregion

		#region PXDatabaseSymbols
		internal static readonly string PXDatabase = "PX.Data.PXDatabase";
		#endregion

		#region PXGraphSymbols
		public static readonly string PXGraph = "PX.Data.PXGraph";
		public static readonly string PXGraph1 = "PX.Data.PXGraph`1";
		public static readonly string PXGraph2 = "PX.Data.PXGraph`2";
		public static readonly string PXGraph3 = "PX.Data.PXGraph`3";
		#endregion

		#region PXIntListAttributeSymbols
		internal static readonly string PXIntListAttribute = "PX.Data.PXIntListAttribute";
		#endregion

		#region PXProcessingBaseSymbols
		internal static readonly string PXProcessingBase = "PX.Data.PXProcessingBase`1";
		#endregion

		#region PXSelectBaseGenericSymbols
		internal static readonly string PXSelectBase1 = "PX.Data.PXSelectBase`1";
		#endregion

		#region PXSelectorAttribute
		internal static readonly string PXSelectorAttribute = "PX.Data.PXSelectorAttribute";
		#endregion

		#region PXStringListAttribute
		internal static readonly string PXStringListAttribute = "PX.Data.PXStringListAttribute";
		#endregion

		#region PXViewSymbols
		internal static readonly string PXView = "PX.Data.PXView";
		#endregion

		#region BqlContext.cs
		public static readonly string SelectBase5 = "PX.Data.SelectBase`5";
		public static readonly string SearchBase5 = "PX.Data.SearchBase`5";
		public static readonly string PXSelectBase = "PX.Data.PXSelectBase";
		public static readonly string Where2 = "PX.Data.Where2`2";
		public static readonly string And2 = "PX.Data.And2`2";
		public static readonly string Or2 = "PX.Data.Or2`2";
		public static readonly string Aggregate = "PX.Data.Aggregate`1";
		public static readonly string GroupByBase2 = "PX.Data.GroupByBase`2";
		public static readonly string BqlPredicateBinaryBase2 = "PX.Data.BqlPredicateBinaryBase`2";
		public static readonly string IBqlCreator = "PX.Data.IBqlCreator";
		public static readonly string IBqlSelect = "PX.Data.IBqlSelect";
		public static readonly string IBqlSearch = "PX.Data.IBqlSearch";
		public static readonly string IBqlJoin = "PX.Data.IBqlJoin";
		public static readonly string IBqlOn = "PX.Data.IBqlOn";
		public static readonly string IBqlWhere = "PX.Data.IBqlWhere";
		public static readonly string IBqlOrderBy = "PX.Data.IBqlOrderBy";
		public static readonly string IBqlSortColumn = "PX.Data.IBqlSortColumn";
		public static readonly string IBqlFunction = "PX.Data.IBqlFunction";
		public static readonly string IBqlPredicateChain = "PX.Data.IBqlPredicateChain";
		public static readonly string IBqlTable = "PX.Data.IBqlTable";
		#endregion

		public static readonly string PXSelectBase_Acumatica2018R2 = "PX.Data.PXSelectBase`2";
		public static readonly string IViewConfig_Acumatica2018R2 = "PX.Data.PXSelectBase`2+IViewConfig";
	}
}