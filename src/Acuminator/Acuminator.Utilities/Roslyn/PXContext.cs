using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn
{
	public class PXContext
	{
		private const string PXSelectBase_Acumatica2018R2 = "PX.Data.PXSelectBase`2";
		private const string IViewConfig_Acumatica2018R2 = "PX.Data.PXSelectBase`2+IViewConfig";

		public bool IsAcumatica2018R2 { get; }

		public Compilation Compilation { get; }

		private readonly Lazy<BQLSymbols> bql;
		public BQLSymbols BQL => bql.Value;

		private readonly Lazy<EventSymbols> events;
		public EventSymbols Events => events.Value;

		private readonly Lazy<FieldAttributesTypes> fieldAttributes;
		public FieldAttributesTypes FieldAttributes => fieldAttributes.Value;

		private readonly Lazy<PXSystemActionTypes> systemActionTypes;
		public PXSystemActionTypes PXSystemActions => systemActionTypes.Value;

		private readonly Lazy<SystemTypeSymbols> systemTypes;
		public SystemTypeSymbols SystemTypes => systemTypes.Value;
		private readonly Lazy<AttributesTypes> attributes;
		public AttributesTypes AttributeTypes => attributes.Value;

        private readonly Lazy<LocalizationTypes> _localizationMethods;
        public LocalizationTypes Localization => _localizationMethods.Value;



		public INamedTypeSymbol PXGraphType => Compilation.GetTypeByMetadataName(typeof(PXGraph).FullName);
		public INamedTypeSymbol PXCacheType => Compilation.GetTypeByMetadataName(typeof(PXCache).FullName);
		public INamedTypeSymbol PXProcessingBaseType => Compilation.GetTypeByMetadataName(typeof(PXProcessingBase<>).FullName);
		public INamedTypeSymbol PXGraphExtensionType => Compilation.GetTypeByMetadataName(typeof(PXGraphExtension).FullName);
		public INamedTypeSymbol PXCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXCacheExtension).FullName);
		public INamedTypeSymbol PXMappedCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXMappedCacheExtension).FullName);
		public INamedTypeSymbol PXViewType => Compilation.GetTypeByMetadataName(typeof(PXView).FullName);
		public INamedTypeSymbol PXSelectBaseType => Compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
		public INamedTypeSymbol PXLongOperation => Compilation.GetTypeByMetadataName(typeof(PXLongOperation).FullName);

		public INamedTypeSymbol PXSelectBase2018R2NewType => Compilation.GetTypeByMetadataName(PXSelectBase_Acumatica2018R2);
		public INamedTypeSymbol IViewConfig2018R2 => Compilation.GetTypeByMetadataName(IViewConfig_Acumatica2018R2);


		public INamedTypeSymbol PXActionType => Compilation.GetTypeByMetadataName(typeof(PXAction).FullName);
		public INamedTypeSymbol PXActionCollection => Compilation.GetTypeByMetadataName(typeof(PXActionCollection).FullName);

		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(typeof(PXAdapter).FullName);
		public INamedTypeSymbol IBqlTableType => Compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);
		public INamedTypeSymbol IBqlFieldType => Compilation.GetTypeByMetadataName(typeof(IBqlField).FullName);

		public INamedTypeSymbol IPXResultsetType => Compilation.GetTypeByMetadataName(typeof(IPXResultset).FullName);
		public INamedTypeSymbol PXResult => Compilation.GetTypeByMetadataName(typeof(PXResult).FullName);

		public INamedTypeSymbol PXFieldState => Compilation.GetTypeByMetadataName(typeof(PXFieldState).FullName);
		public INamedTypeSymbol PXAttributeFamily => Compilation.GetTypeByMetadataName(typeof(PXAttributeFamilyAttribute).FullName);

        public INamedTypeSymbol PXException => Compilation.GetTypeByMetadataName(typeof(PXException).FullName);
        public INamedTypeSymbol PXBaseRedirectException => Compilation.GetTypeByMetadataName(typeof(PXBaseRedirectException).FullName);

        public INamedTypeSymbol IPXLocalizableList => Compilation.GetTypeByMetadataName(typeof(IPXLocalizableList).FullName);
		public INamedTypeSymbol PXConnectionScope => Compilation.GetTypeByMetadataName(typeof(PXConnectionScope).FullName);
		public INamedTypeSymbol PXDatabase => Compilation.GetTypeByMetadataName(typeof(PXDatabase).FullName);
		public INamedTypeSymbol PXSelectorAttribute => Compilation.GetTypeByMetadataName(typeof(PXSelectorAttribute).FullName);

        public ImmutableArray<ISymbol> StringFormat => SystemTypes.String.GetMembers(nameof(string.Format));
        public ImmutableArray<ISymbol> StringConcat => SystemTypes.String.GetMembers(nameof(string.Concat));

		public PXContext(Compilation compilation)
		{
			Compilation = compilation;
			bql = new Lazy<BQLSymbols>(() => new BQLSymbols(Compilation));
			events = new Lazy<EventSymbols>(() => new EventSymbols(Compilation));
			fieldAttributes = new Lazy<FieldAttributesTypes>(
										() => new FieldAttributesTypes(Compilation));
			systemActionTypes = new Lazy<PXSystemActionTypes>(
										() => new PXSystemActionTypes(Compilation));
			attributes = new Lazy<AttributesTypes>(
										() => new AttributesTypes(Compilation));
			systemTypes = new Lazy<SystemTypeSymbols>(
										() => new SystemTypeSymbols(Compilation));
            _localizationMethods = new Lazy<LocalizationTypes>(
                () => new LocalizationTypes(Compilation));

			IsAcumatica2018R2 = PXSelectBase2018R2NewType != null;
		}

		#region System Types
		public class SystemTypeSymbols
		{
			private readonly Compilation _compilation;

			public SystemTypeSymbols(Compilation aCompilation)
			{
				_compilation = aCompilation;
			}

			public INamedTypeSymbol Array => _compilation.GetSpecialType(SpecialType.System_Array);

			public IArrayTypeSymbol ByteArray => _compilation.CreateArrayTypeSymbol(Byte);
			public IArrayTypeSymbol StringArray => _compilation.CreateArrayTypeSymbol(String);

			public INamedTypeSymbol String => _compilation.GetSpecialType(SpecialType.System_String);
			public INamedTypeSymbol Bool => _compilation.GetSpecialType(SpecialType.System_Boolean);
			public INamedTypeSymbol Int64 => _compilation.GetSpecialType(SpecialType.System_Int64);
			public INamedTypeSymbol Int32 => _compilation.GetSpecialType(SpecialType.System_Int32);
			public INamedTypeSymbol Int16 => _compilation.GetSpecialType(SpecialType.System_Int16);
			public INamedTypeSymbol Byte => _compilation.GetSpecialType(SpecialType.System_Byte);
			public INamedTypeSymbol Double => _compilation.GetSpecialType(SpecialType.System_Double);
			public INamedTypeSymbol Float => _compilation.GetSpecialType(SpecialType.System_Single);
			public INamedTypeSymbol Decimal => _compilation.GetSpecialType(SpecialType.System_Decimal);
			public INamedTypeSymbol DateTime => _compilation.GetSpecialType(SpecialType.System_DateTime);
			public INamedTypeSymbol Nullable => _compilation.GetSpecialType(SpecialType.System_Nullable_T);
            public INamedTypeSymbol Enum => _compilation.GetSpecialType(SpecialType.System_Enum);

			public INamedTypeSymbol IEnumerable => _compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);

			public INamedTypeSymbol Guid => _compilation.GetTypeByMetadataName(typeof(Guid).FullName);
            public INamedTypeSymbol TimeSpan => _compilation.GetTypeByMetadataName(typeof(TimeSpan).FullName);
        }
		#endregion

		#region Field Attributes Types
		public class FieldAttributesTypes
		{
			private readonly Compilation _compilation;

            public FieldAttributesTypes(Compilation aCompilation)
            {
                _compilation = aCompilation;
            }

			public INamedTypeSymbol PXDBScalarAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBScalarAttribute).FullName);

			#region Field Unbound Attributes
			public INamedTypeSymbol PXLongAttribute => _compilation.GetTypeByMetadataName(typeof(PXLongAttribute).FullName);
            public INamedTypeSymbol PXIntAttribute => _compilation.GetTypeByMetadataName(typeof(PXIntAttribute).FullName);
            public INamedTypeSymbol PXShortAttribute => _compilation.GetTypeByMetadataName(typeof(PXShortAttribute).FullName);
            public INamedTypeSymbol PXStringAttribute => _compilation.GetTypeByMetadataName(typeof(PXStringAttribute).FullName);
            public INamedTypeSymbol PXByteAttribute => _compilation.GetTypeByMetadataName(typeof(PXByteAttribute).FullName);
            public INamedTypeSymbol PXDecimalAttribute => _compilation.GetTypeByMetadataName(typeof(PXDecimalAttribute).FullName);
            public INamedTypeSymbol PXDoubleAttribute => _compilation.GetTypeByMetadataName(typeof(PXDoubleAttribute).FullName);

			public INamedTypeSymbol PXFloatAttribute => _compilation.GetTypeByMetadataName(typeof(PXFloatAttribute).FullName);
			public INamedTypeSymbol PXDateAttribute => _compilation.GetTypeByMetadataName(typeof(PXDateAttribute).FullName);
            public INamedTypeSymbol PXGuidAttribute => _compilation.GetTypeByMetadataName(typeof(PXGuidAttribute).FullName);
            public INamedTypeSymbol PXBoolAttribute => _compilation.GetTypeByMetadataName(typeof(PXBoolAttribute).FullName);			
            #endregion

            #region DBField Attributes
            public INamedTypeSymbol PXDBFieldAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBFieldAttribute).FullName);

            public INamedTypeSymbol PXDBLongAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBLongAttribute).FullName);
            public INamedTypeSymbol PXDBIntAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBIntAttribute).FullName);
            public INamedTypeSymbol PXDBShortAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBShortAttribute).FullName);
            public INamedTypeSymbol PXDBStringAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBStringAttribute).FullName);
            public INamedTypeSymbol PXDBByteAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBByteAttribute).FullName);
            public INamedTypeSymbol PXDBDecimalAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBDecimalAttribute).FullName);
            public INamedTypeSymbol PXDBDoubleAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBDoubleAttribute).FullName);
            public INamedTypeSymbol PXDBFloatAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBFloatAttribute).FullName);
            public INamedTypeSymbol PXDBDateAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBDateAttribute).FullName);
            public INamedTypeSymbol PXDBGuidAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBGuidAttribute).FullName);
            public INamedTypeSymbol PXDBBoolAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBBoolAttribute).FullName);
            public INamedTypeSymbol PXDBTimestampAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBTimestampAttribute).FullName);

            public INamedTypeSymbol PXDBIdentityAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBIdentityAttribute).FullName);
            public INamedTypeSymbol PXDBLongIdentityAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBLongIdentityAttribute).FullName);
            public INamedTypeSymbol PXDBBinaryAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBBinaryAttribute).FullName);
            public INamedTypeSymbol PXDBUserPasswordAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBUserPasswordAttribute).FullName);
			public INamedTypeSymbol PXDBCalcedAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBCalcedAttribute).FullName);
			public INamedTypeSymbol PXDBAttributeAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBAttributeAttribute).FullName);
			public INamedTypeSymbol PXDBDataLengthAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBDataLengthAttribute).FullName);
			#endregion
		}
		#endregion
		#region Attributes Types
		public class AttributesTypes
		{
			private readonly Compilation _compilation;

			public AttributesTypes(Compilation aCompilation)
			{
				_compilation = aCompilation;
			}


			public INamedTypeSymbol PXImportAttribute => _compilation.GetTypeByMetadataName(typeof(PXImportAttribute).FullName);
			public INamedTypeSymbol PXHiddenAttribute => _compilation.GetTypeByMetadataName(typeof(PXHiddenAttribute).FullName);
			public INamedTypeSymbol PXCopyPasteHiddenViewAttribute => _compilation.GetTypeByMetadataName(typeof(PXCopyPasteHiddenViewAttribute).FullName);

			public INamedTypeSymbol PXStringListAttribute => _compilation.GetTypeByMetadataName(typeof(PXStringListAttribute).FullName);
			public INamedTypeSymbol PXIntListAttribute => _compilation.GetTypeByMetadataName(typeof(PXIntListAttribute).FullName);

			public INamedTypeSymbol PXEventSubscriberAttribute => _compilation.GetTypeByMetadataName(typeof(PXEventSubscriberAttribute).FullName);
			public INamedTypeSymbol PXAttributeFamily => _compilation.GetTypeByMetadataName(typeof(PXAttributeFamilyAttribute).FullName);
			public INamedTypeSymbol PXAggregateAttribute => _compilation.GetTypeByMetadataName(typeof(PXAggregateAttribute).FullName);
			public INamedTypeSymbol PXDynamicAggregateAttribute => _compilation.GetTypeByMetadataName(typeof(PXDynamicAggregateAttribute).FullName);
			public INamedTypeSymbol PXDefaultAttribute => _compilation.GetTypeByMetadataName(typeof(PXDefaultAttribute).FullName);
			public INamedTypeSymbol PXUnboundDefaultAttribute => _compilation.GetTypeByMetadataName(typeof(PXUnboundDefaultAttribute).FullName);

		}
		#endregion

		#region System Actions Types
		public class PXSystemActionTypes
		{
			private readonly Compilation _compilation;

			public PXSystemActionTypes(Compilation aCompilation)
			{
				_compilation = aCompilation;
			}

			public INamedTypeSymbol PXSave => _compilation.GetTypeByMetadataName(typeof(PXSave<>).FullName);

			public INamedTypeSymbol PXCancel => _compilation.GetTypeByMetadataName(typeof(PXCancel<>).FullName);

			public INamedTypeSymbol PXInsert => _compilation.GetTypeByMetadataName(typeof(PXInsert<>).FullName);

			public INamedTypeSymbol PXDelete => _compilation.GetTypeByMetadataName(typeof(PXDelete<>).FullName);

			public INamedTypeSymbol PXCopyPasteAction => _compilation.GetTypeByMetadataName(typeof(PXCopyPasteAction<>).FullName);

			public INamedTypeSymbol PXFirst => _compilation.GetTypeByMetadataName(typeof(PXFirst<>).FullName);

			public INamedTypeSymbol PXPrevious => _compilation.GetTypeByMetadataName(typeof(PXPrevious<>).FullName);

			public INamedTypeSymbol PXNext => _compilation.GetTypeByMetadataName(typeof(PXNext<>).FullName);

			public INamedTypeSymbol PXLast => _compilation.GetTypeByMetadataName(typeof(PXLast<>).FullName);
		}
		#endregion

		#region BQL Types
		/// <summary>
		/// BQL Symbols are stored in separate file.
		/// </summary>
		public class BQLSymbols
		{
			private readonly Compilation _compilation;

			public BQLSymbols(Compilation aCompilation)
			{
				_compilation = aCompilation;
			}

			#region CustomDelegates
			public INamedTypeSymbol CustomPredicate => _compilation.GetTypeByMetadataName(typeof(CustomPredicate).FullName);

			public INamedTypeSymbol AreSame => _compilation.GetTypeByMetadataName(typeof(AreSame<,>).FullName);

			public INamedTypeSymbol AreDistinct => _compilation.GetTypeByMetadataName(typeof(AreDistinct<,>).FullName);
			#endregion

			public INamedTypeSymbol Required => _compilation.GetTypeByMetadataName(typeof(Required<>).FullName);

			public INamedTypeSymbol Argument => _compilation.GetTypeByMetadataName(typeof(Argument<>).FullName);

			public INamedTypeSymbol Optional => _compilation.GetTypeByMetadataName(typeof(PX.Data.Optional<>).FullName);
			public INamedTypeSymbol Optional2 => _compilation.GetTypeByMetadataName(typeof(Optional2<>).FullName);

			public INamedTypeSymbol BqlCommand => _compilation.GetTypeByMetadataName(typeof(BqlCommand).FullName);

			public INamedTypeSymbol IBqlParameter => _compilation.GetTypeByMetadataName(typeof(IBqlParameter).FullName);
			
			public INamedTypeSymbol PXSelectBaseGenericType => _compilation.GetTypeByMetadataName(typeof(PXSelectBase<>).FullName);

			public INamedTypeSymbol PXFilter => _compilation.GetTypeByMetadataName(typeof(PXFilter<>).FullName);

			public INamedTypeSymbol IPXNonUpdateable => _compilation.GetTypeByMetadataName(typeof(IPXNonUpdateable).FullName); 

			#region PXSetup
			public INamedTypeSymbol PXSetup => _compilation.GetTypeByMetadataName(typeof(PXSetup<>).FullName);

			public INamedTypeSymbol PXSetupWhere => _compilation.GetTypeByMetadataName(typeof(PXSetup<,>).FullName);

			public INamedTypeSymbol PXSetupJoin => _compilation.GetTypeByMetadataName(typeof(PXSetup<,,>).FullName);

			public INamedTypeSymbol PXSetupSelect => _compilation.GetTypeByMetadataName(typeof(PXSetupSelect<>).FullName);

			public ImmutableArray<INamedTypeSymbol> GetPXSetupTypes() =>
				ImmutableArray.Create
				(
					PXSetup,
					PXSetupWhere,
					PXSetupJoin,
					PXSetupSelect
				);
			#endregion
		}
		#endregion

		#region EventSymbols

		public class EventSymbols
		{
			private readonly Compilation _compilation;

			public EventSymbols(Compilation compilation)
			{
				_compilation = compilation;
				_eventTypeMap = new Lazy<IReadOnlyDictionary<ITypeSymbol, EventType>>(
					() => CreateEventTypeMap(this));
				_eventHandlerSignatureTypeMap = new Lazy<IReadOnlyDictionary<(EventType, EventHandlerSignatureType),INamedTypeSymbol>>(
					() => CreateEventHandlerSignatureTypeMap(this));
			}

			private readonly Lazy<IReadOnlyDictionary<ITypeSymbol, EventType>> _eventTypeMap;
			public IReadOnlyDictionary<ITypeSymbol, EventType> EventTypeMap => _eventTypeMap.Value;

			private readonly Lazy<IReadOnlyDictionary<(EventType, EventHandlerSignatureType), INamedTypeSymbol>> _eventHandlerSignatureTypeMap;
			public IReadOnlyDictionary<(EventType, EventHandlerSignatureType), INamedTypeSymbol> EventHandlerSignatureTypeMap => _eventHandlerSignatureTypeMap.Value;

			public INamedTypeSymbol PXRowSelectingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowSelectingEventArgs).FullName);
			public INamedTypeSymbol PXRowSelectedEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowSelectedEventArgs).FullName);
			public INamedTypeSymbol PXRowInsertingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowInsertingEventArgs).FullName);
			public INamedTypeSymbol PXRowInsertedEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowInsertedEventArgs).FullName);
			public INamedTypeSymbol PXRowUpdatingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowUpdatingEventArgs).FullName);
			public INamedTypeSymbol PXRowUpdatedEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowUpdatedEventArgs).FullName);
			public INamedTypeSymbol PXRowDeletingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowDeletingEventArgs).FullName);
			public INamedTypeSymbol PXRowDeletedEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowDeletedEventArgs).FullName);
			public INamedTypeSymbol PXRowPersistingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowPersistingEventArgs).FullName);
			public INamedTypeSymbol PXRowPersistedEventArgs => _compilation.GetTypeByMetadataName(typeof(PXRowPersistedEventArgs).FullName);

			public INamedTypeSymbol PXFieldSelectingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXFieldSelectingEventArgs).FullName);
			public INamedTypeSymbol PXFieldDefaultingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXFieldDefaultingEventArgs).FullName);
			public INamedTypeSymbol PXFieldVerifyingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXFieldVerifyingEventArgs).FullName);
			public INamedTypeSymbol PXFieldUpdatingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXFieldUpdatingEventArgs).FullName);
			public INamedTypeSymbol PXFieldUpdatedEventArgs => _compilation.GetTypeByMetadataName(typeof(PXFieldUpdatedEventArgs).FullName);
			public INamedTypeSymbol PXCommandPreparingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXCommandPreparingEventArgs).FullName);
			public INamedTypeSymbol PXExceptionHandlingEventArgs => _compilation.GetTypeByMetadataName(typeof(PXExceptionHandlingEventArgs).FullName);

			public INamedTypeSymbol CacheAttached => _compilation.GetTypeByMetadataName(typeof(Events.CacheAttached<>).FullName);
			public INamedTypeSymbol RowSelecting => _compilation.GetTypeByMetadataName(typeof(Events.RowSelecting<>).FullName);
			public INamedTypeSymbol RowSelected => _compilation.GetTypeByMetadataName(typeof(Events.RowSelected<>).FullName);
			public INamedTypeSymbol RowInserting => _compilation.GetTypeByMetadataName(typeof(Events.RowInserting<>).FullName);
			public INamedTypeSymbol RowInserted => _compilation.GetTypeByMetadataName(typeof(Events.RowInserted<>).FullName);
			public INamedTypeSymbol RowUpdating => _compilation.GetTypeByMetadataName(typeof(Events.RowUpdating<>).FullName);
			public INamedTypeSymbol RowUpdated => _compilation.GetTypeByMetadataName(typeof(Events.RowUpdated<>).FullName);
			public INamedTypeSymbol RowDeleting => _compilation.GetTypeByMetadataName(typeof(Events.RowDeleting<>).FullName);
			public INamedTypeSymbol RowDeleted => _compilation.GetTypeByMetadataName(typeof(Events.RowDeleted<>).FullName);
			public INamedTypeSymbol RowPersisting => _compilation.GetTypeByMetadataName(typeof(Events.RowPersisting<>).FullName);
			public INamedTypeSymbol RowPersisted => _compilation.GetTypeByMetadataName(typeof(Events.RowPersisted<>).FullName);

			public INamedTypeSymbol FieldSelecting => _compilation.GetTypeByMetadataName(typeof(Events.FieldSelecting<>).FullName);
			public INamedTypeSymbol FieldDefaulting => _compilation.GetTypeByMetadataName(typeof(Events.FieldDefaulting<>).FullName);
			public INamedTypeSymbol FieldVerifying => _compilation.GetTypeByMetadataName(typeof(Events.FieldVerifying<>).FullName);
			public INamedTypeSymbol FieldUpdating => _compilation.GetTypeByMetadataName(typeof(Events.FieldUpdating<>).FullName);
			public INamedTypeSymbol FieldUpdated => _compilation.GetTypeByMetadataName(typeof(Events.FieldUpdated<>).FullName);
			public INamedTypeSymbol CommandPreparing => _compilation.GetTypeByMetadataName(typeof(Events.CommandPreparing<>).FullName);
			public INamedTypeSymbol ExceptionHandling => _compilation.GetTypeByMetadataName(typeof(Events.ExceptionHandling<>).FullName);

			private static IReadOnlyDictionary<ITypeSymbol, EventType> CreateEventTypeMap(EventSymbols eventSymbols)
			{
				return new Dictionary<ITypeSymbol, EventType>()
				{
					{ eventSymbols.PXRowSelectingEventArgs, EventType.RowSelecting },
					{ eventSymbols.PXRowSelectedEventArgs, EventType.RowSelected },
					{ eventSymbols.PXRowInsertingEventArgs, EventType.RowInserting },
					{ eventSymbols.PXRowInsertedEventArgs, EventType.RowInserted },
					{ eventSymbols.PXRowUpdatingEventArgs, EventType.RowUpdating },
					{ eventSymbols.PXRowUpdatedEventArgs, EventType.RowUpdated },
					{ eventSymbols.PXRowDeletingEventArgs, EventType.RowDeleting },
					{ eventSymbols.PXRowDeletedEventArgs, EventType.RowDeleted },
					{ eventSymbols.PXRowPersistingEventArgs, EventType.RowPersisting },
					{ eventSymbols.PXRowPersistedEventArgs, EventType.RowPersisted },
					{ eventSymbols.PXFieldSelectingEventArgs, EventType.FieldSelecting },
					{ eventSymbols.PXFieldDefaultingEventArgs, EventType.FieldDefaulting },
					{ eventSymbols.PXFieldVerifyingEventArgs, EventType.FieldVerifying },
					{ eventSymbols.PXFieldUpdatingEventArgs, EventType.FieldUpdating },
					{ eventSymbols.PXFieldUpdatedEventArgs, EventType.FieldUpdated },
					{ eventSymbols.PXCommandPreparingEventArgs, EventType.CommandPreparing },
					{ eventSymbols.PXExceptionHandlingEventArgs, EventType.ExceptionHandling },

					{ eventSymbols.CacheAttached, EventType.CacheAttached },
					{ eventSymbols.RowSelecting, EventType.RowSelecting },
					{ eventSymbols.RowSelected, EventType.RowSelected },
					{ eventSymbols.RowInserting, EventType.RowInserting },
					{ eventSymbols.RowInserted, EventType.RowInserted },
					{ eventSymbols.RowUpdating, EventType.RowUpdating },
					{ eventSymbols.RowUpdated, EventType.RowUpdated },
					{ eventSymbols.RowDeleting, EventType.RowDeleting },
					{ eventSymbols.RowDeleted, EventType.RowDeleted },
					{ eventSymbols.RowPersisting, EventType.RowPersisting },
					{ eventSymbols.RowPersisted, EventType.RowPersisted },
					{ eventSymbols.FieldSelecting, EventType.FieldSelecting },
					{ eventSymbols.FieldDefaulting, EventType.FieldDefaulting },
					{ eventSymbols.FieldVerifying, EventType.FieldVerifying },
					{ eventSymbols.FieldUpdating, EventType.FieldUpdating },
					{ eventSymbols.FieldUpdated, EventType.FieldUpdated },
					{ eventSymbols.CommandPreparing, EventType.CommandPreparing },
					{ eventSymbols.ExceptionHandling, EventType.ExceptionHandling },
				};
			}

			private static IReadOnlyDictionary<(EventType, EventHandlerSignatureType), INamedTypeSymbol>
				CreateEventHandlerSignatureTypeMap(EventSymbols eventSymbols)
			{
				return new Dictionary<(EventType, EventHandlerSignatureType), INamedTypeSymbol>()
				{
					{ (EventType.RowSelecting, EventHandlerSignatureType.Default), eventSymbols.PXRowSelectingEventArgs },
					{ (EventType.RowSelected, EventHandlerSignatureType.Default), eventSymbols.PXRowSelectedEventArgs },
					{ (EventType.RowInserting, EventHandlerSignatureType.Default), eventSymbols.PXRowInsertingEventArgs },
					{ (EventType.RowInserted, EventHandlerSignatureType.Default), eventSymbols.PXRowInsertedEventArgs },
					{ (EventType.RowUpdating, EventHandlerSignatureType.Default), eventSymbols.PXRowUpdatingEventArgs },
					{ (EventType.RowUpdated, EventHandlerSignatureType.Default), eventSymbols.PXRowUpdatedEventArgs },
					{ (EventType.RowDeleting, EventHandlerSignatureType.Default), eventSymbols.PXRowDeletingEventArgs },
					{ (EventType.RowDeleted, EventHandlerSignatureType.Default), eventSymbols.PXRowDeletedEventArgs },
					{ (EventType.RowPersisting, EventHandlerSignatureType.Default), eventSymbols.PXRowPersistingEventArgs },
					{ (EventType.RowPersisted, EventHandlerSignatureType.Default), eventSymbols.PXRowPersistedEventArgs },
					{ (EventType.FieldSelecting, EventHandlerSignatureType.Default), eventSymbols.PXFieldSelectingEventArgs },
					{ (EventType.FieldDefaulting, EventHandlerSignatureType.Default), eventSymbols.PXFieldDefaultingEventArgs },
					{ (EventType.FieldVerifying, EventHandlerSignatureType.Default), eventSymbols.PXFieldVerifyingEventArgs },
					{ (EventType.FieldUpdating, EventHandlerSignatureType.Default), eventSymbols.PXFieldUpdatingEventArgs },
					{ (EventType.FieldUpdated, EventHandlerSignatureType.Default), eventSymbols.PXFieldUpdatedEventArgs },
					{ (EventType.CommandPreparing, EventHandlerSignatureType.Default), eventSymbols.PXCommandPreparingEventArgs },
					{ (EventType.ExceptionHandling, EventHandlerSignatureType.Default), eventSymbols.PXExceptionHandlingEventArgs },

					{ (EventType.CacheAttached, EventHandlerSignatureType.Generic), eventSymbols.CacheAttached },
					{ (EventType.RowSelecting, EventHandlerSignatureType.Generic), eventSymbols.RowSelecting },
					{ (EventType.RowSelected, EventHandlerSignatureType.Generic), eventSymbols.RowSelected },
					{ (EventType.RowInserting, EventHandlerSignatureType.Generic), eventSymbols.RowInserting },
					{ (EventType.RowInserted, EventHandlerSignatureType.Generic), eventSymbols.RowInserted },
					{ (EventType.RowUpdating, EventHandlerSignatureType.Generic), eventSymbols.RowUpdating },
					{ (EventType.RowUpdated, EventHandlerSignatureType.Generic), eventSymbols.RowUpdated },
					{ (EventType.RowDeleting, EventHandlerSignatureType.Generic), eventSymbols.RowDeleting },
					{ (EventType.RowDeleted, EventHandlerSignatureType.Generic), eventSymbols.RowDeleted },
					{ (EventType.RowPersisting, EventHandlerSignatureType.Generic), eventSymbols.RowPersisting },
					{ (EventType.RowPersisted, EventHandlerSignatureType.Generic), eventSymbols.RowPersisted },
					{ (EventType.FieldSelecting, EventHandlerSignatureType.Generic), eventSymbols.FieldSelecting },
					{ (EventType.FieldDefaulting, EventHandlerSignatureType.Generic), eventSymbols.FieldDefaulting },
					{ (EventType.FieldVerifying, EventHandlerSignatureType.Generic), eventSymbols.FieldVerifying },
					{ (EventType.FieldUpdating, EventHandlerSignatureType.Generic), eventSymbols.FieldUpdating },
					{ (EventType.FieldUpdated, EventHandlerSignatureType.Generic), eventSymbols.FieldUpdated },
					{ (EventType.CommandPreparing, EventHandlerSignatureType.Generic), eventSymbols.CommandPreparing },
					{ (EventType.ExceptionHandling, EventHandlerSignatureType.Generic), eventSymbols.ExceptionHandling },
				};
			}
		}

		#endregion
	}
}
