using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;


namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public partial class PXGraphEventSemanticModel
	{
		private readonly CancellationToken _cancellation;
		private readonly PXContext _pxContext;

		public PXGraphSemanticModel BaseGraphModel { get; }

		#region Base Model
		public bool IsProcessing => BaseGraphModel.IsProcessing;

		public GraphType Type => BaseGraphModel.Type;

		public INamedTypeSymbol Symbol => BaseGraphModel.Symbol;

		/// <summary>
		/// The graph symbol. For the graph is the same as <see cref="Symbol"/>. For graph extensions is the extension's base graph.
		/// </summary>
		public ITypeSymbol GraphSymbol => BaseGraphModel.GraphSymbol;

		public ImmutableArray<StaticConstructorInfo> StaticConstructors => BaseGraphModel.StaticConstructors;
		public ImmutableArray<GraphInitializerInfo> Initializers => BaseGraphModel.Initializers;

		public ImmutableDictionary<string, DataViewInfo> ViewsByNames => BaseGraphModel.ViewsByNames;
		public IEnumerable<DataViewInfo> Views => BaseGraphModel.Views;

		public ImmutableDictionary<string, DataViewDelegateInfo> ViewDelegatesByNames => BaseGraphModel.ViewDelegatesByNames;
		public IEnumerable<DataViewDelegateInfo> ViewDelegates => BaseGraphModel.ViewDelegates;

		public ImmutableDictionary<string, ActionInfo> ActionsByNames => BaseGraphModel.ActionsByNames;
		public IEnumerable<ActionInfo> Actions => BaseGraphModel.Actions;

		public ImmutableDictionary<string, ActionHandlerInfo> ActionHandlersByNames => BaseGraphModel.ActionHandlersByNames;
		public IEnumerable<ActionHandlerInfo> ActionHandlers => BaseGraphModel.ActionHandlers;
		#endregion

		#region Events
		public ImmutableDictionary<string, GraphEventInfo> RowSelectingByName { get; }
		public IEnumerable<GraphEventInfo> RowSelectingEvents => RowSelectingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowSelectedByName { get; }
		public IEnumerable<GraphEventInfo> RowSelectedEvents => RowSelectedByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowInsertingByName { get; }
		public IEnumerable<GraphEventInfo> RowInsertingEvents => RowInsertingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowInsertedByName { get; }
		public IEnumerable<GraphEventInfo> RowInsertedEvents => RowInsertedByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowUpdatingByName { get; }
		public IEnumerable<GraphEventInfo> RowUpdatingEvents => RowUpdatingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowUpdatedByName { get; }
		public IEnumerable<GraphEventInfo> RowUpdatedEvents => RowUpdatedByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowDeletingByName { get; }
		public IEnumerable<GraphEventInfo> RowDeletingEvents => RowDeletingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowDeletedByName { get; }
		public IEnumerable<GraphEventInfo> RowDeletedEvents => RowDeletedByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowPersistingByName { get; }
		public IEnumerable<GraphEventInfo> RowPersistingEvents => RowPersistingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowPersistedByName { get; }
		public IEnumerable<GraphEventInfo> RowPersistedEvents => RowPersistedByName.Values;

		public ImmutableDictionary<string, GraphFieldEventInfo> FieldSelectingByName { get; }
		public IEnumerable<GraphFieldEventInfo> FieldSelectingEvents => FieldSelectingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventInfo> FieldDefaultingByName { get; }
		public IEnumerable<GraphFieldEventInfo> FieldDefaultingEvents => FieldDefaultingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventInfo> FieldVerifyingByName { get; }
		public IEnumerable<GraphFieldEventInfo> FieldVerifyingEvents => FieldVerifyingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventInfo> FieldUpdatingByName { get; }
		public IEnumerable<GraphFieldEventInfo> FieldUpdatingEvents => FieldUpdatingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventInfo> FieldUpdatedByName { get; }
		public IEnumerable<GraphFieldEventInfo> FieldUpdatedEvents => FieldUpdatedByName.Values;

		public ImmutableDictionary<string, GraphFieldEventInfo> CacheAttachedByName { get; }
		public IEnumerable<GraphFieldEventInfo> CacheAttachedEvents => CacheAttachedByName.Values;

		public ImmutableDictionary<string, GraphFieldEventInfo> CommandPreparingByName { get; }
		public IEnumerable<GraphFieldEventInfo> CommandPreparingEvents => CommandPreparingByName.Values;

		public ImmutableDictionary<string, GraphFieldEventInfo> ExceptionHandlingByName { get; }
		public IEnumerable<GraphFieldEventInfo> ExceptionHandlingEvents => ExceptionHandlingByName.Values;
		#endregion


		private PXGraphEventSemanticModel(PXContext pxContext, PXGraphSemanticModel baseGraphModel,
									      CancellationToken cancellation = default)
		{
			_pxContext = pxContext;
			_cancellation = cancellation;
			BaseGraphModel = baseGraphModel;

			if (BaseGraphModel.Type != GraphType.None)
			{
				var eventsCollector = InitializeEvents();
			
				RowSelectingByName = GetRowEvents(eventsCollector, EventType.RowSelecting);
				RowSelectedByName = GetRowEvents(eventsCollector, EventType.RowSelected);

				RowInsertingByName = GetRowEvents(eventsCollector, EventType.RowInserting);
				RowInsertedByName = GetRowEvents(eventsCollector, EventType.RowInserted);

				RowUpdatingByName = GetRowEvents(eventsCollector, EventType.RowUpdating);
				RowUpdatedByName = GetRowEvents(eventsCollector, EventType.RowUpdated);

				RowDeletingByName = GetRowEvents(eventsCollector, EventType.RowDeleting);
				RowDeletedByName = GetRowEvents(eventsCollector, EventType.RowDeleted);

				RowPersistingByName = GetRowEvents(eventsCollector, EventType.RowPersisting);
				RowPersistedByName = GetRowEvents(eventsCollector, EventType.RowPersisted);

				FieldSelectingByName = GetFieldEvents(eventsCollector, EventType.FieldSelecting);
				FieldDefaultingByName = GetFieldEvents(eventsCollector, EventType.FieldDefaulting);
				FieldVerifyingByName = GetFieldEvents(eventsCollector, EventType.FieldVerifying);
				FieldUpdatingByName = GetFieldEvents(eventsCollector, EventType.FieldUpdating);
				FieldUpdatedByName = GetFieldEvents(eventsCollector, EventType.FieldUpdated);

				CacheAttachedByName = GetFieldEvents(eventsCollector, EventType.CacheAttached);
				CommandPreparingByName = GetFieldEvents(eventsCollector, EventType.CommandPreparing);
				ExceptionHandlingByName = GetFieldEvents(eventsCollector, EventType.ExceptionHandling);
			}
		}

		public static IEnumerable<PXGraphEventSemanticModel> InferModels(PXContext pxContext, INamedTypeSymbol typeSymbol,
																		 CancellationToken cancellation = default)
		{	
			var baseGraphModels = PXGraphSemanticModel.InferModels(pxContext, typeSymbol, cancellation);
			var eventsGraphModels = baseGraphModels.Select(graph => new PXGraphEventSemanticModel(pxContext, graph, cancellation))
												   .ToList();
			return eventsGraphModels;
		}

		private EventsCollector InitializeEvents()
		{
			_cancellation.ThrowIfCancellationRequested();
			var methods = GetAllGraphMethodsFromBaseToDerived();

			var eventsCollector = new EventsCollector(this, _pxContext);
			int declarationOrder = 0;

			foreach (IMethodSymbol method in methods)
			{
				_cancellation.ThrowIfCancellationRequested();

				var (eventType, eventSignatureType) = method.GetEventHandlerInfo(_pxContext);

				if (eventSignatureType == EventHandlerSignatureType.None || eventType == EventType.None)
					continue;

				eventsCollector.AddEvent(eventSignatureType, eventType, method, declarationOrder, _cancellation);
				declarationOrder++;
			}

			return eventsCollector;
		}


		private IEnumerable<IMethodSymbol> GetAllGraphMethodsFromBaseToDerived()
		{
			IEnumerable<ITypeSymbol> baseTypes = BaseGraphModel.GraphSymbol
															   .GetGraphWithBaseTypes()
															   .Reverse();

			if (BaseGraphModel.Type == GraphType.PXGraphExtension)
			{
				baseTypes = baseTypes.Concat(
										BaseGraphModel.Symbol.GetGraphExtensionWithBaseExtensions(_pxContext, 
																								  SortDirection.Ascending,
																								  includeGraph: false));
			}

			return baseTypes.SelectMany(t => t.GetMembers().OfType<IMethodSymbol>());
		}

		private ImmutableDictionary<string, GraphEventInfo> GetRowEvents(EventsCollector eventsCollector, EventType eventType)
		{
			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, GraphEventInfo>();

			GraphEventsCollection<GraphEventInfo> rawCollection = eventsCollector.GetRowEvents(eventType);
			return rawCollection.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? 
				   ImmutableDictionary.Create<string, GraphEventInfo>();
		}

		private ImmutableDictionary<string, GraphFieldEventInfo> GetFieldEvents(EventsCollector eventsCollector, EventType eventType)
		{
			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, GraphFieldEventInfo>();

			GraphEventsCollection<GraphFieldEventInfo> rawCollection = eventsCollector.GetFieldEvents(eventType);
			return rawCollection.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value) ??
				   ImmutableDictionary.Create<string, GraphFieldEventInfo>();
		}
	}
}
