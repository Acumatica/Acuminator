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
		public ImmutableDictionary<string, GraphEventInfo> CacheAttachedByDacName { get; }
		public IEnumerable<GraphEventInfo> CacheAttachedEvents => CacheAttachedByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowSelectingByDacName { get; }
		public IEnumerable<GraphEventInfo> RowSelectingEvents => RowSelectingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowSelectedByDacName { get; }
		public IEnumerable<GraphEventInfo> RowSelectedEvents => RowSelectedByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowInsertingByDacName { get; }
		public IEnumerable<GraphEventInfo> RowInsertingEvents => RowInsertingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowInsertedByDacName { get; }
		public IEnumerable<GraphEventInfo> RowInsertedEvents => RowInsertedByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowUpdatingByDacName { get; }
		public IEnumerable<GraphEventInfo> RowUpdatingEvents => RowUpdatingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowUpdatedByDacName { get; }
		public IEnumerable<GraphEventInfo> RowUpdatedEvents => RowUpdatedByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowDeletingByDacName { get; }
		public IEnumerable<GraphEventInfo> RowDeletingEvents => RowDeletingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowDeletedByDacName { get; }
		public IEnumerable<GraphEventInfo> RowDeletedEvents => RowDeletedByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowPersistingByDacName { get; }
		public IEnumerable<GraphEventInfo> RowPersistingEvents => RowPersistingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> RowPersistedByDacName { get; }
		public IEnumerable<GraphEventInfo> RowPersistedEvents => RowPersistedByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldSelectingByDacName { get; }
		public IEnumerable<GraphEventInfo> FieldSelectingEvents => FieldSelectingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldDefaultingByDacName { get; }
		public IEnumerable<GraphEventInfo> FieldDefaultingEvents => FieldDefaultingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldVerifyingByDacName { get; }
		public IEnumerable<GraphEventInfo> FieldVerifyingEvents => FieldVerifyingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldUpdatingByDacName { get; }
		public IEnumerable<GraphEventInfo> FieldUpdatingEvents => FieldUpdatingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> FieldUpdatedByDacName { get; }
		public IEnumerable<GraphEventInfo> FieldUpdatedEvents => FieldUpdatedByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> CommandPreparingByDacName { get; }
		public IEnumerable<GraphEventInfo> CommandPreparingEvents => CommandPreparingByDacName.Values;

		public ImmutableDictionary<string, GraphEventInfo> ExceptionHandlingByDacName { get; }
		public IEnumerable<GraphEventInfo> ExceptionHandlingEvents => ExceptionHandlingByDacName.Values;
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

				CacheAttachedByDacName = GetEvents(eventsCollector, collector => collector.CacheAttachedEvents);
				RowSelectingByDacName = GetEvents(eventsCollector, collector => collector.RowSelectingEvents);
				RowSelectedByDacName = GetEvents(eventsCollector, collector => collector.RowSelectedEvents);
				RowInsertingByDacName = GetEvents(eventsCollector, collector => collector.RowInsertingEvents);
				RowInsertedByDacName = GetEvents(eventsCollector, collector => collector.RowInsertedEvents);
				RowUpdatingByDacName = GetEvents(eventsCollector, collector => collector.RowUpdatingEvents);
				RowUpdatedByDacName = GetEvents(eventsCollector, collector => collector.RowUpdatedEvents);
				RowDeletingByDacName = GetEvents(eventsCollector, collector => collector.RowDeletingEvents);
				RowDeletedByDacName = GetEvents(eventsCollector, collector => collector.RowDeletedEvents);
				RowPersistingByDacName = GetEvents(eventsCollector, collector => collector.RowPersistingEvents);
				RowPersistedByDacName = GetEvents(eventsCollector, collector => collector.RowPersistedEvents);
				FieldSelectingByDacName = GetEvents(eventsCollector, collector => collector.FieldSelectingEvents);
				FieldDefaultingByDacName = GetEvents(eventsCollector, collector => collector.FieldDefaultingEvents);
				FieldVerifyingByDacName = GetEvents(eventsCollector, collector => collector.FieldVerifyingEvents);
				FieldUpdatingByDacName = GetEvents(eventsCollector, collector => collector.FieldUpdatingEvents);
				FieldUpdatedByDacName = GetEvents(eventsCollector, collector => collector.FieldUpdatedEvents);
				CommandPreparingByDacName = GetEvents(eventsCollector, collector => collector.CommandPreparingEvents);
				ExceptionHandlingByDacName = GetEvents(eventsCollector, collector => collector.ExceptionHandlingEvents);
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
