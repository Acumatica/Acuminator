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
		private class EventsCollector
		{
			private readonly PXContext _pxContext;
			private readonly PXGraphEventSemanticModel _graphEventSemanticModel;

			public GraphOverridableItemsCollection<GraphEventInfoBase> CacheAttachedEvents { get; } =
				new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowSelectingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowSelectedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowInsertingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowInsertedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowUpdatingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowUpdatedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowDeletingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowDeletedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowPersistingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> RowPersistedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> FieldSelectingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> FieldDefaultingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> FieldVerifyingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> FieldUpdatingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> FieldUpdatedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> CommandPreparingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public GraphOverridableItemsCollection<GraphEventInfoBase> ExceptionHandlingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfoBase>();

			public EventsCollector(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
			{
				_pxContext = context;
				_graphEventSemanticModel = graphEventSemanticModel;
			}

			public void AddEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
								 int declarationOrder, CancellationToken cancellationToken)
			{
				if (methodSymbol.DeclaringSyntaxReferences.Length != 1)
					return;

				var methodNode = methodSymbol.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax;

				if (methodNode == null)
					return;

				GraphOverridableItemsCollection<GraphEventInfoBase> colectionToAdd = GetCollectionToAdd(eventType);

				if (colectionToAdd == null)
					return;

				GraphEventInfoBase eventToAdd = GetEventToAdd(signatureType, eventType, methodNode, methodSymbol, declarationOrder);
				string eventKey = eventToAdd.GetEventGroupingKey();
				colectionToAdd.Add(eventKey, eventToAdd, declarationOrder);
			}

			private GraphOverridableItemsCollection<GraphEventInfoBase> GetCollectionToAdd(EventType eventType)
			{
				switch (eventType)
				{
					case EventType.CacheAttached:
						return CacheAttachedEvents;

					case EventType.RowSelecting:
						return RowSelectingEvents;

					case EventType.RowSelected:
						return RowSelectedEvents;

					case EventType.RowInserting:
						return RowInsertingEvents;

					case EventType.RowInserted:
						return RowInsertedEvents;

					case EventType.RowUpdating:
						return RowUpdatingEvents;

					case EventType.RowUpdated:
						return RowUpdatedEvents;

					case EventType.RowDeleting:
						return RowDeletingEvents;

					case EventType.RowDeleted:
						return RowDeletedEvents;

					case EventType.RowPersisting:
						return RowPersistingEvents;

					case EventType.RowPersisted:
						return RowPersistedEvents;

					case EventType.FieldSelecting:
						return FieldSelectingEvents;

					case EventType.FieldDefaulting:
						return FieldDefaultingEvents;

					case EventType.FieldVerifying:
						return FieldVerifyingEvents;

					case EventType.FieldUpdating:
						return FieldUpdatingEvents;

					case EventType.FieldUpdated:
						return FieldUpdatedEvents;

					default:
						return null;
				}
			}

			private GraphEventInfoBase GetEventToAdd(EventHandlerSignatureType signatureType, EventType eventType, MethodDeclarationSyntax methodNode,
													 IMethodSymbol methodSymbol, int declarationOrder)
			{
				if (eventType.IsDacFieldEvent())
				{
					return new GraphFieldEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);
				}
				else
				{
					return new GraphEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);
				}
			}
		}
	}
}
