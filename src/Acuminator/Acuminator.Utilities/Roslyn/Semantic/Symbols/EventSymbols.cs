using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class EventSymbols
	{
		private readonly Compilation _compilation;

		internal EventSymbols(Compilation compilation)
		{
			_compilation = compilation;
			_eventTypeMap = new Lazy<IReadOnlyDictionary<ITypeSymbol, EventType>>(
				() => CreateEventTypeMap(this));
			_eventHandlerSignatureTypeMap = new Lazy<IReadOnlyDictionary<(EventType, EventHandlerSignatureType), INamedTypeSymbol>>(
				() => CreateEventHandlerSignatureTypeMap(this));
		}

		private readonly Lazy<IReadOnlyDictionary<ITypeSymbol, EventType>> _eventTypeMap;
		public IReadOnlyDictionary<ITypeSymbol, EventType> EventTypeMap => _eventTypeMap.Value;

		private readonly Lazy<IReadOnlyDictionary<(EventType, EventHandlerSignatureType), INamedTypeSymbol>> _eventHandlerSignatureTypeMap;
		public IReadOnlyDictionary<(EventType, EventHandlerSignatureType), INamedTypeSymbol> EventHandlerSignatureTypeMap => _eventHandlerSignatureTypeMap.Value;

		public INamedTypeSymbol PXRowSelectingEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowSelectingEventArgs);
		public INamedTypeSymbol PXRowSelectedEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowSelectedEventArgs);
		public INamedTypeSymbol PXRowInsertingEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowInsertingEventArgs);
		public INamedTypeSymbol PXRowInsertedEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowInsertedEventArgs);
		public INamedTypeSymbol PXRowUpdatingEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowUpdatingEventArgs);
		public INamedTypeSymbol PXRowUpdatedEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowUpdatedEventArgs);
		public INamedTypeSymbol PXRowDeletingEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowDeletingEventArgs);
		public INamedTypeSymbol PXRowDeletedEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowDeletedEventArgs);
		public INamedTypeSymbol PXRowPersistingEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowPersistingEventArgs);
		public INamedTypeSymbol PXRowPersistedEventArgs => _compilation.GetTypeByMetadataName(Types.PXRowPersistedEventArgs);

		public INamedTypeSymbol PXFieldSelectingEventArgs => _compilation.GetTypeByMetadataName(Types.PXFieldSelectingEventArgs);
		public INamedTypeSymbol PXFieldDefaultingEventArgs => _compilation.GetTypeByMetadataName(Types.PXFieldDefaultingEventArgs);
		public INamedTypeSymbol PXFieldVerifyingEventArgs => _compilation.GetTypeByMetadataName(Types.PXFieldVerifyingEventArgs);
		public INamedTypeSymbol PXFieldUpdatingEventArgs => _compilation.GetTypeByMetadataName(Types.PXFieldUpdatingEventArgs);
		public INamedTypeSymbol PXFieldUpdatedEventArgs => _compilation.GetTypeByMetadataName(Types.PXFieldUpdatedEventArgs);
		public INamedTypeSymbol PXCommandPreparingEventArgs => _compilation.GetTypeByMetadataName(Types.PXCommandPreparingEventArgs);
		public INamedTypeSymbol PXExceptionHandlingEventArgs => _compilation.GetTypeByMetadataName(Types.PXExceptionHandlingEventArgs);

		public INamedTypeSymbol CacheAttached => _compilation.GetTypeByMetadataName(Types.Events.CacheAttached);
		public INamedTypeSymbol RowSelecting => _compilation.GetTypeByMetadataName(Types.Events.RowSelecting);
		public INamedTypeSymbol RowSelected => _compilation.GetTypeByMetadataName(Types.Events.RowSelected);
		public INamedTypeSymbol RowInserting => _compilation.GetTypeByMetadataName(Types.Events.RowInserting);
		public INamedTypeSymbol RowInserted => _compilation.GetTypeByMetadataName(Types.Events.RowInserted);
		public INamedTypeSymbol RowUpdating => _compilation.GetTypeByMetadataName(Types.Events.RowUpdating);
		public INamedTypeSymbol RowUpdated => _compilation.GetTypeByMetadataName(Types.Events.RowUpdated);
		public INamedTypeSymbol RowDeleting => _compilation.GetTypeByMetadataName(Types.Events.RowDeleting);
		public INamedTypeSymbol RowDeleted => _compilation.GetTypeByMetadataName(Types.Events.RowDeleted);
		public INamedTypeSymbol RowPersisting => _compilation.GetTypeByMetadataName(Types.Events.RowPersisting);
		public INamedTypeSymbol RowPersisted => _compilation.GetTypeByMetadataName(Types.Events.RowPersisted);

		public INamedTypeSymbol FieldSelecting => _compilation.GetTypeByMetadataName(Types.Events.FieldSelecting);
		public INamedTypeSymbol FieldDefaulting => _compilation.GetTypeByMetadataName(Types.Events.FieldDefaulting);
		public INamedTypeSymbol FieldVerifying => _compilation.GetTypeByMetadataName(Types.Events.FieldVerifying);
		public INamedTypeSymbol FieldUpdating => _compilation.GetTypeByMetadataName(Types.Events.FieldUpdating);
		public INamedTypeSymbol FieldUpdated => _compilation.GetTypeByMetadataName(Types.Events.FieldUpdated);
		public INamedTypeSymbol CommandPreparing => _compilation.GetTypeByMetadataName(Types.Events.CommandPreparing);
		public INamedTypeSymbol ExceptionHandling => _compilation.GetTypeByMetadataName(Types.Events.ExceptionHandling1);

		public INamedTypeSymbol FieldSelectingTypedRow => _compilation.GetTypeByMetadataName(Types.Events.FieldSelecting2);
		public INamedTypeSymbol FieldDefaultingTypedRow => _compilation.GetTypeByMetadataName(Types.Events.FieldDefaulting2);
		public INamedTypeSymbol FieldVerifyingTypedRow => _compilation.GetTypeByMetadataName(Types.Events.FieldVerifying2);
		public INamedTypeSymbol FieldUpdatingTypedRow => _compilation.GetTypeByMetadataName(Types.Events.FieldUpdating2);
		public INamedTypeSymbol FieldUpdatedTypedRow => _compilation.GetTypeByMetadataName(Types.Events.FieldUpdated2);
		public INamedTypeSymbol ExceptionHandlingTypedRow => _compilation.GetTypeByMetadataName(Types.Events.ExceptionHandling2);

		private static IReadOnlyDictionary<ITypeSymbol, EventType> CreateEventTypeMap(EventSymbols eventSymbols)
		{
			var map = new Dictionary<ITypeSymbol, EventType>()
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

			// These symbols can be absent on some versions of Acumatica
			map.TryAdd(eventSymbols.FieldSelectingTypedRow, EventType.FieldSelecting);
			map.TryAdd(eventSymbols.FieldDefaultingTypedRow, EventType.FieldDefaulting);
			map.TryAdd(eventSymbols.FieldVerifyingTypedRow, EventType.FieldVerifying);
			map.TryAdd(eventSymbols.FieldUpdatingTypedRow, EventType.FieldUpdating);
			map.TryAdd(eventSymbols.FieldUpdatedTypedRow, EventType.FieldUpdated);
			map.TryAdd(eventSymbols.ExceptionHandlingTypedRow, EventType.ExceptionHandling);

			return map;
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
					{ (EventType.FieldSelecting, EventHandlerSignatureType.Generic), eventSymbols.FieldSelectingTypedRow ?? eventSymbols.FieldSelecting },
					{ (EventType.FieldDefaulting, EventHandlerSignatureType.Generic), eventSymbols.FieldDefaultingTypedRow ?? eventSymbols.FieldDefaulting },
					{ (EventType.FieldVerifying, EventHandlerSignatureType.Generic), eventSymbols.FieldVerifyingTypedRow ?? eventSymbols.FieldVerifying },
					{ (EventType.FieldUpdating, EventHandlerSignatureType.Generic), eventSymbols.FieldUpdatingTypedRow ?? eventSymbols.FieldUpdating },
					{ (EventType.FieldUpdated, EventHandlerSignatureType.Generic), eventSymbols.FieldUpdatedTypedRow ?? eventSymbols.FieldUpdated },
					{ (EventType.CommandPreparing, EventHandlerSignatureType.Generic), eventSymbols.CommandPreparing },
					{ (EventType.ExceptionHandling, EventHandlerSignatureType.Generic), eventSymbols.ExceptionHandlingTypedRow ?? eventSymbols.ExceptionHandlingTypedRow },
				};
		}
	}
}
