using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using PX.Data;

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

		public INamedTypeSymbol FieldSelectingTypedRow => _compilation.GetTypeByMetadataName(typeof(Events.FieldSelecting<,>).FullName);
		public INamedTypeSymbol FieldDefaultingTypedRow => _compilation.GetTypeByMetadataName(typeof(Events.FieldDefaulting<,>).FullName);
		public INamedTypeSymbol FieldVerifyingTypedRow => _compilation.GetTypeByMetadataName(typeof(Events.FieldVerifying<,>).FullName);
		public INamedTypeSymbol FieldUpdatingTypedRow => _compilation.GetTypeByMetadataName(typeof(Events.FieldUpdating<,>).FullName);
		public INamedTypeSymbol FieldUpdatedTypedRow => _compilation.GetTypeByMetadataName(typeof(Events.FieldUpdated<,>).FullName);
		public INamedTypeSymbol ExceptionHandlingTypedRow => _compilation.GetTypeByMetadataName(typeof(Events.ExceptionHandling<,>).FullName);

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
