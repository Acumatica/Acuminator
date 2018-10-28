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
		public INamedTypeSymbol GraphSymbol => BaseGraphModel.GraphSymbol;

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
		public ImmutableDictionary<string, GraphEventInfo> CacheAttachedByName { get; }
		public IEnumerable<GraphEventInfo> CacheAttachedEvents => CacheAttachedByName.Values;

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

		public ImmutableDictionary<string, GraphEventInfo> FieldSelectingByName { get; }
		public IEnumerable<GraphEventInfo> FieldSelectingEvents => FieldSelectingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldDefaultingByName { get; }
		public IEnumerable<GraphEventInfo> FieldDefaultingEvents => FieldDefaultingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldVerifyingByName { get; }
		public IEnumerable<GraphEventInfo> FieldVerifyingEvents => FieldVerifyingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldUpdatingByName { get; }
		public IEnumerable<GraphEventInfo> FieldUpdatingEvents => FieldUpdatingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldUpdatedByName { get; }
		public IEnumerable<GraphEventInfo> FieldUpdatedEvents => FieldUpdatedByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> CommandPreparingByName { get; }
		public IEnumerable<GraphEventInfo> CommandPreparingEvents => CommandPreparingByName.Values;

		public ImmutableDictionary<string, GraphEventInfo> ExceptionHandlingByName { get; }
		public IEnumerable<GraphEventInfo> ExceptionHandlingEvents => ExceptionHandlingByName.Values;
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

				CacheAttachedByName = GetEvents(eventsCollector, collector => collector.CacheAttachedEvents);
				RowSelectingByName = GetEvents(eventsCollector, collector => collector.RowSelectingEvents);
				RowSelectedByName = GetEvents(eventsCollector, collector => collector.RowSelectedEvents);
				RowInsertingByName = GetEvents(eventsCollector, collector => collector.RowInsertingEvents);
				RowInsertedByName = GetEvents(eventsCollector, collector => collector.RowInsertedEvents);
				RowUpdatingByName = GetEvents(eventsCollector, collector => collector.RowUpdatingEvents);
				RowUpdatedByName = GetEvents(eventsCollector, collector => collector.RowUpdatedEvents);
				RowDeletingByName = GetEvents(eventsCollector, collector => collector.RowDeletingEvents);
				RowDeletedByName = GetEvents(eventsCollector, collector => collector.RowDeletedEvents);
				RowPersistingByName = GetEvents(eventsCollector, collector => collector.RowPersistingEvents);
				RowPersistedByName = GetEvents(eventsCollector, collector => collector.RowPersistedEvents);
				FieldSelectingByName = GetEvents(eventsCollector, collector => collector.FieldSelectingEvents);
				FieldDefaultingByName = GetEvents(eventsCollector, collector => collector.FieldDefaultingEvents);
				FieldVerifyingByName = GetEvents(eventsCollector, collector => collector.FieldVerifyingEvents);
				FieldUpdatingByName = GetEvents(eventsCollector, collector => collector.FieldUpdatingEvents);
				FieldUpdatedByName = GetEvents(eventsCollector, collector => collector.FieldUpdatedEvents);
				CommandPreparingByName = GetEvents(eventsCollector, collector => collector.CommandPreparingEvents);
				ExceptionHandlingByName = GetEvents(eventsCollector, collector => collector.ExceptionHandlingEvents);
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
															   .GetBaseTypesAndThis()
															   .TakeWhile(baseGraph => !baseGraph.IsGraphBaseType())
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

		private ImmutableDictionary<string, GraphEventInfo> GetEvents(EventsCollector eventsCollector, 
																	  Func<EventsCollector, GraphOverridableItemsCollection<GraphEventInfo>> selector)
		{
			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, GraphEventInfo>(StringComparer.OrdinalIgnoreCase);

			var rawCollection = selector(eventsCollector);
			return rawCollection.Values.ToLookup(e => e.Item.Symbol.Name, StringComparer.OrdinalIgnoreCase)
									   .ToImmutableDictionary(group => group.Key,
															  group => CreateEventInfo(group.First()),
															  keyComparer: StringComparer.OrdinalIgnoreCase);


			GraphEventInfo CreateEventInfo(GraphOverridableItem<GraphEventInfo> item)
			{
				GraphEventInfo eventInfo = item.Item;

				GraphEventInfo baseEventInfo = item.Base != null
					? CreateEventInfo(item.Base)
					: null;

				return baseEventInfo == null
					? new GraphEventInfo(eventInfo.Node, eventInfo.Symbol, item.DeclarationOrder, eventInfo.SignatureType, eventInfo.EventType)
					: new GraphEventInfo(eventInfo.Node, eventInfo.Symbol, item.DeclarationOrder, eventInfo.SignatureType, eventInfo.EventType,
										 baseEventInfo);
			}
		}
	}
}
