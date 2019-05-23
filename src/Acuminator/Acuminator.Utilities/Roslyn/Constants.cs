using System;

namespace Acuminator.Utilities.Roslyn
{
	internal static class Constants
	{
		internal static class Types
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
			internal static readonly string PXUIFieldAttributeSetVisible = "SetVisible";
			internal static readonly string PXUIFieldAttributeSetVisibility = "SetVisibility";
			internal static readonly string PXUIFieldAttributeSetEnabled = "SetEnabled";
			internal static readonly string PXUIFieldAttributeSetRequired = "SetRequired";
			internal static readonly string PXUIFieldAttributeSetReadOnly = "SetReadOnly";
			internal static readonly string PXUIFieldAttributeSetDisplayName = "SetDisplayName";
			internal static readonly string PXUIFieldAttributeSetNeutralDisplayName = "SetNeutralDisplayName";
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
			internal static readonly string IBqlTable = "PX.Data.IBqlTable";
			internal static readonly string IBqlField = "PX.Data.IBqlField";
			internal static readonly string Constant = "PX.Data.Constant`1";
			internal static readonly string IPXResultset = "PX.Data.IPXResultset";
			internal static readonly string PXResult = "PX.Data.PXResult";
			internal static readonly string PXFieldState = "PX.Data.PXFieldState";
			internal static readonly string PXAttributeFamilyAttribute = "PX.Data.PXAttributeFamilyAttribute";
			internal static readonly string IPXLocalizableList = "PX.Data.IPXLocalizableList";
			internal static readonly string PXConnectionScope = "PX.Data.PXConnectionScope";

			internal static readonly string PXGraphExtensionInitialize = "Initialize";
			internal static readonly string PXLongOperationStartOperation = "StartOperation";

			internal static readonly string IImplementType = "PX.Common.IImplement`1";
			#endregion

			#region AttributeSymbol.cs
			internal static readonly string PXImportAttribute = "PX.Data.PXImportAttribute";
			internal static readonly string PXHiddenAttribute = "PX.Data.PXHiddenAttribute";
			internal static readonly string PXCacheNameAttribute = "PX.Data.PXCacheNameAttribute";
			internal static readonly string PXCopyPasteHiddenViewAttribute = "PX.Data.PXCopyPasteHiddenViewAttribute";
			internal static readonly string PXOverrideAttribute = "PX.Data.PXOverrideAttribute";
			internal static readonly string PXEventSubscriberAttribute = "PX.Data.PXEventSubscriberAttribute";
			//internal static readonly string PXAttributeFamilyAttribute = "PX.Data.PXAttributeFamilyAttribute"; // dublicate
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

			#region BQLSymbols.cs
			internal static readonly string CustomPredicate = "PX.Data.CustomPredicate";
			internal static readonly string AreSame2 = "PX.Data.AreSame`2";
			internal static readonly string AreDistinct2 = "PX.Data.AreDistinct`2";
			internal static readonly string Required1 = "PX.Data.Required`1";
			internal static readonly string Argument1 = "PX.Data.Argument`1";
			internal static readonly string Optional1 = "PX.Data.Optional`1";
			internal static readonly string Optional2 = "PX.Data.Optional2`1";
			internal static readonly string BqlCommand = "PX.Data.BqlCommand";
			internal static readonly string IBqlParameter = "PX.Data.IBqlParameter";
			//internal static readonly string PXSelectBase1 = "PX.Data.PXSelectBase`1";//dublicate
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

			#region EventSymbols.cs

			internal static readonly string PXRowSelectingEventArgs = "PX.Data.PXRowSelectingEventArgs";
			internal static readonly string PXRowSelectedEventArgs = "PX.Data.PXRowSelectedEventArgs";
			internal static readonly string PXRowInsertingEventArgs = "PX.Data.PXRowInsertingEventArgs";
			internal static readonly string PXRowInsertedEventArgs = "PX.Data.PXRowInsertedEventArgs";
			internal static readonly string PXRowUpdatingEventArgs = "PX.Data.PXRowUpdatingEventArgs";
			internal static readonly string PXRowUpdatedEventArgs = "PX.Data.PXRowUpdatedEventArgs";
			internal static readonly string PXRowDeletingEventArgs = "PX.Data.PXRowDeletingEventArgs";
			internal static readonly string PXRowDeletedEventArgs = "PX.Data.PXRowDeletedEventArgs";
			internal static readonly string PXRowPersistingEventArgs = "PX.Data.PXRowPersistingEventArgs";
			internal static readonly string PXRowPersistedEventArgs = "PX.Data.PXRowPersistedEventArgs";
			internal static readonly string PXFieldSelectingEventArgs = "PX.Data.PXFieldSelectingEventArgs";
			internal static readonly string PXFieldDefaultingEventArgs = "PX.Data.PXFieldDefaultingEventArgs";
			internal static readonly string PXFieldVerifyingEventArgs = "PX.Data.PXFieldVerifyingEventArgs";
			internal static readonly string PXFieldUpdatingEventArgs = "PX.Data.PXFieldUpdatingEventArgs";
			internal static readonly string PXFieldUpdatedEventArgs = "PX.Data.PXFieldUpdatedEventArgs";
			internal static readonly string PXCommandPreparingEventArgs = "PX.Data.PXCommandPreparingEventArgs";
			internal static readonly string PXExceptionHandlingEventArgs = "PX.Data.PXExceptionHandlingEventArgs";

			internal static class Events { 
				internal static readonly string CacheAttached = "PX.Data.Events+CacheAttached`1";
				internal static readonly string RowSelecting = "PX.Data.Events+RowSelecting`1";
				internal static readonly string RowSelected = "PX.Data.Events+RowSelected`1";
				internal static readonly string RowInserting = "PX.Data.Events+RowInserting`1";
				internal static readonly string RowInserted = "PX.Data.Events+RowInserted`1";
				internal static readonly string RowUpdating = "PX.Data.Events+RowUpdating`1";
				internal static readonly string RowUpdated = "PX.Data.Events+RowUpdated`1";
				internal static readonly string RowDeleting = "PX.Data.Events+RowDeleting`1";
				internal static readonly string RowDeleted = "PX.Data.Events+RowDeleted`1";
				internal static readonly string RowPersisting = "PX.Data.Events+RowPersisting`1";
				internal static readonly string RowPersisted = "PX.Data.Events+RowPersisted`1";
				internal static readonly string FieldSelecting = "PX.Data.Events+FieldSelecting`1";
				internal static readonly string FieldDefaulting = "PX.Data.Events+FieldDefaulting`1";
				internal static readonly string FieldVerifying = "PX.Data.Events+FieldVerifying`1";
				internal static readonly string FieldUpdating = "PX.Data.Events+FieldUpdating`1";
				internal static readonly string FieldUpdated = "PX.Data.Events+FieldUpdated`1";
				internal static readonly string CommandPreparing = "PX.Data.Events+CommandPreparing`1";
				internal static readonly string ExceptionHandling1 = "PX.Data.Events+ExceptionHandling`1";
				internal static readonly string FieldSelecting2 = "PX.Data.Events+FieldSelecting`2";
				internal static readonly string FieldDefaulting2 = "PX.Data.Events+FieldDefaulting`2";
				internal static readonly string FieldVerifying2 = "PX.Data.Events+FieldVerifying`2";
				internal static readonly string FieldUpdating2 = "PX.Data.Events+FieldUpdating`2";
				internal static readonly string FieldUpdated2 = "PX.Data.Events+FieldUpdated`2";
				internal static readonly string CommandPreparing2 = "PX.Data.Events+CommandPreparing`2";
				internal static readonly string ExceptionHandling2 = "PX.Data.Events+ExceptionHandling`2";

			}
			#endregion

			#region ExceprionSymbols.cs
			internal static readonly string PXException = "PX.Data.PXException";
			internal static readonly string PXBaseRedirectException = "PX.Data.PXBaseRedirectException";
			internal static readonly string PXSetupNotEnteredException = "PX.Data.PXSetupNotEnteredException";
			#endregion

			#region PXActionSymbol.cs

			internal static readonly string PXAction = "PX.Data.PXAction";

			internal static class PXActionDelegates
			{
				internal static readonly string SetVisible = "SetVisible";
				internal static readonly string SetEnabled = "SetEnabled";
				internal static readonly string SetCaption = "SetCaption";
				internal static readonly string SetTooltip = "SetTooltip";
				internal static readonly string Press = "Press";
			}

			#endregion

			#region PXCacheSymbols.cs
			
			internal static readonly string PXCache = "PX.Data.PXCache";

			internal static class PXCacheDelegates
			{
				[GenerateConstString("nameof(PX.Data.PXCache.Insert)")]
				internal static readonly string Insert = "Insert";
				
				[GenerateConstString("nameof(PX.Data.PXCache.Update)")]
				internal static readonly string Update = "Update";

				[GenerateConstString("nameof(PX.Data.PXCache.Delete)")]
				internal static readonly string Delete = "Delete";

				[GenerateConstString("nameof(PX.Data.PXCache.RaiseExceptionHandling)")]
				internal static readonly string RaiseExceptionHandling = "RaiseExceptionHandling";
			}
			#endregion

			#region PXDatabaseSymbols.cs
			internal static readonly string PXDatabase = "PX.Data.PXDatabase";


			internal static class PXDatabaseDelegates
			{
				internal static readonly string Select = "Select";
				internal static readonly string Insert = "Insert";
				internal static readonly string Update = "Update";
				internal static readonly string Delete = "Delete";
				internal static readonly string ForceDelete = "ForceDelete";
				internal static readonly string Ensure = "Ensure";
			}

			#endregion

			#region PXGraphSymbols.cs

			internal static readonly string PXGraph = "PX.Data.PXGraph";
			internal static readonly string PXGraph1 = "PX.Data.PXGraph`1";
			internal static readonly string PXGraph2 = "PX.Data.PXGraph`2";
			internal static readonly string PXGraph3 = "PX.Data.PXGraph`3";

			internal static class PXGraphDelegates
			{
				internal static readonly string InstanceCreatedEvents = "PX.Data.PXGraph+InstanceCreatedEvents";
				internal static readonly string InstanceCreatedEventsAddHabdler = "AddHandler";
				internal static readonly string InitCacheMapping = "InitCacheMapping";
				internal static readonly string CreateInstance = "CreateInstance";
			}

			#endregion
			
			#region PXIntListAttributeSymbols

			internal static readonly string PXIntListAttribute = "PX.Data.PXIntListAttribute";
			
			internal static class PXIntListAttributeDelegates
			{
				internal static readonly string SetList = "SetList";
			}

			#endregion
			
			#region PXProcessingBaseSymbols

			internal static readonly string PXProcessingBase = "PX.Data.PXProcessingBase`1";
			
			internal static class PXProcessingBaseDelegates
			{
				internal static readonly string SetParameters = "SetParametersDelegate";
				internal static readonly string SetProcess = "SetProcessDelegate";
			}

			#endregion

			#region PXSelectBaseGenericSymbols

			internal static readonly string PXSelectBase1 = "PX.Data.PXSelectBase`1";
			
			internal static class PXSelectBase1Delegates
			{
				internal static readonly string Insert = "Insert";
				internal static readonly string Update = "Update";
				internal static readonly string Delete = "Delete";
				internal static readonly string Select = "Select";
			}

			#endregion

			#region PXSelectBase
			
			internal static readonly string  PXSelectBase = "PX.Data.PXSelectBase";
			
			internal static class PXSelectBaseDelegates
			{
				internal static readonly string View = "View";

				internal static readonly string WhereAnd = "WhereAnd";
				internal static readonly string WhereNew = "WhereNew";
				internal static readonly string WhereOr = "WhereOr";
				internal static readonly string Join = "Join";
			}

			#endregion

			#region PXSelectorAttribute

			internal static readonly string PXSelectorAttribute = "PX.Data.PXSelectorAttribute";
			
			internal static class PXSelectorAttributeDelegates
			{
				internal static readonly string Select = "Select";
				internal static readonly string GetItem = "GetItem";
			}

			#endregion

			#region PXStringListAttribute

			internal static readonly string PXStringListAttribute = "PX.Data.PXStringListAttribute";
			
			internal static class PXStringListAttributeDelegates
			{
				internal static readonly string SetList = "SetList";
				internal static readonly string AppendList = "AppendList";
				internal static readonly string SetLocalizable = "SetLocalizable";
			}

			#endregion
			#region PXSystemActionSymbols
			internal static class PXSystemActionSymbols
			{
				internal static readonly string PXSave = "PX.Data.PXSave`1";
				internal static readonly string PXCancel = "PX.Data.PXCancel`1";
				internal static readonly string PXInsert = "PX.Data.PXInsert`1";
				internal static readonly string PXDelete = "PX.Data.PXDelete`1";
				internal static readonly string PXCopyPasteAction = "PX.Data.PXCopyPasteAction`1";
				internal static readonly string PXFirst = "PX.Data.PXFirst`1";
				internal static readonly string PXPrevious = "PX.Data.PXPrevious`1";
				internal static readonly string PXNext = "PX.Data.PXNext`1";
				internal static readonly string PXLast = "PX.Data.PXLast`1";
				internal static readonly string PXChangeID = "PX.Data.PXChangeID`1";
			}

			#endregion

			#region PXViewSymbols
			internal static readonly string PXView = "PX.Data.PXView";
			
			internal static class PXViewDelegates
			{
				internal static readonly string Select = "Select";

				internal static readonly string WhereAnd = "WhereAnd";
				internal static readonly string WhereNew = "WhereNew";
				internal static readonly string WhereOr = "WhereOr";
				internal static readonly string Join = "Join";
				internal static readonly string JoinNew = "JoinNew";
			}
			#endregion

			#region BqlModifyingMethods.cs
			internal static class BqlCommandDelegates
			{
				#region BqlCommandInstance
				internal static readonly string WhereAnd = "WhereAnd";
				internal static readonly string WhereNew = "WhereNew";
				internal static readonly string WhereOr = "WhereOr";
				internal static readonly string AggregateNew = "AggregateNew";
				internal static readonly string OrderByNew = "OrderByNew";
				#endregion

				#region BqlCommandStatic
				internal static readonly string Compose = "Compose";
				internal static readonly string CreateInstance = "CreateInstance";
				internal static readonly string AddJoinConditions = "AddJoinConditions";
				internal static readonly string AppendJoin = "AppendJoin";
				internal static readonly string NewJoin = "NewJoin";
				#endregion
			}
			#endregion

			/*#region TypeNames.cs
			internal static readonly string IBqlTable = "IBqlTable";

			internal static readonly string BqlCommand = "BqlCommand";

			internal static readonly string IBqlField = "IBqlField";
			internal static readonly string IBqlParameter = "IBqlParameter";
			internal static readonly string IBqlJoin = "IBqlJoin";
			internal static readonly string IBqlOrderBy = "IBqlOrderBy";
			internal static readonly string IBqlAggregate = "IBqlAggregate";
			internal static readonly string IBqlFunction = "IBqlFunction";
			internal static readonly string IBqlSortColumn = "IBqlSortColumn";
			internal static readonly string IBqlComparison = "IBqlComparison";
			internal static readonly string IBqlCreator = "IBqlCreator";
			internal static readonly string IBqlPredicateChain = "IBqlPredicateChain";
			internal static readonly string IBqlOn = "IBqlOn";
			internal static readonly string IBqlSet = "IBqlSet";

			internal static readonly string PXSelectBaseType = "PXSelectBase";

			internal static readonly string Constant = "Constant";
			internal static readonly string ConstantGeneric = "Constant`1";

			internal static readonly string PXCacheExtension = "PXCacheExtension";
			internal static readonly string PXCacheExtensionGeneric = "PXCacheExtension`1";

			internal static readonly string PXGraph = "PXGraph";

			internal static readonly string PXGraphGeneric = "PXGraph`1";
			internal static readonly string PXGraphExtension = "PXGraphExtension";

			internal static readonly string PXAction = "PXAction";
			internal static readonly string PXActionGeneric = "PXAction`1";

			internal const string PXUpdate = nameof(PXUpdate);
			internal const string PXUpdateJoin = nameof(PXUpdateJoin);
			internal const string PXUpdateGroupBy = nameof(PXUpdateGroupBy);
			internal const string PXUpdateJoinGroupBy = nameof(PXUpdateJoinGroupBy);

			internal const string PXSelectReadonly = "PXSelectReadonly";
			internal const string PXSelectReadonly2 = nameof(PXSelectReadonly2);
			internal const string PXSelectReadonly3 = nameof(PXSelectReadonly3);

			#endregion*/

			#region Template

			internal static readonly string TemplateAttributeOrClass = "typeof(AcumaticaType).FullName";
			
			internal static class TemplateDelegates
			{
				internal static readonly string SetList = "SetList";
			}

			#endregion


		}

		internal sealed class GenerateConstStringAttribute : Attribute
		{
			public GenerateConstStringAttribute(string name)
			{

			}
		}

	}
}
