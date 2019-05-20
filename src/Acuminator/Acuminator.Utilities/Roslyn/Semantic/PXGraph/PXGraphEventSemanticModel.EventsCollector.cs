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

			public GraphEventsCollection<GraphEventInfo> CacheAttachedEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowSelectingEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowSelectedEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowInsertingEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowInsertedEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowUpdatingEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowUpdatedEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowDeletingEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowDeletedEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowPersistingEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphEventInfo> RowPersistedEvents { get; } = new GraphEventsCollection<GraphEventInfo>();

			public GraphEventsCollection<GraphFieldEventInfo> FieldSelectingEvents { get; } = new GraphEventsCollection<GraphFieldEventInfo>();

			public GraphEventsCollection<GraphFieldEventInfo> FieldDefaultingEvents { get; } = new GraphEventsCollection<GraphFieldEventInfo>();

			public GraphEventsCollection<GraphFieldEventInfo> FieldVerifyingEvents { get; } = new GraphEventsCollection<GraphFieldEventInfo>();

			public GraphEventsCollection<GraphFieldEventInfo> FieldUpdatingEvents { get; } = new GraphEventsCollection<GraphFieldEventInfo>();

			public GraphEventsCollection<GraphFieldEventInfo> FieldUpdatedEvents { get; } = new GraphEventsCollection<GraphFieldEventInfo>();

			public GraphEventsCollection<GraphFieldEventInfo> CommandPreparingEvents { get; } = new GraphEventsCollection<GraphFieldEventInfo>();

			public GraphEventsCollection<GraphFieldEventInfo> ExceptionHandlingEvents { get; } = new GraphEventsCollection<GraphFieldEventInfo>();

			public EventsCollector(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
			{
				_pxContext = context;
				_graphEventSemanticModel = graphEventSemanticModel;
			}

			public void AddEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
								 int declarationOrder, CancellationToken cancellationToken)
			{
				var methodNode = GetMethodNode(methodSymbol, cancellationToken);
				if (methodNode == null)
					return;

				GraphEventsCollection<GraphEventInfo> collectionToAdd = GetEventsCollectionToAdd(eventType);
				if (collectionToAdd == null)
					return;

				GraphEventInfo eventToAdd = new GraphEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);
				string eventKey = eventToAdd.GetEventGroupingKey();
				collectionToAdd.AddEventInfo(eventKey, eventToAdd);
			}

			public void AddFieldEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
									  int declarationOrder, CancellationToken cancellationToken)
			{
				var methodNode = GetMethodNode(methodSymbol, cancellationToken);
				if (methodNode == null)
					return;

				GraphEventsCollection<GraphFieldEventInfo> collectionToAdd = GetFieldEventsCollectionToAdd(eventType);
				if (collectionToAdd == null)
					return;

				GraphFieldEventInfo eventToAdd = new GraphFieldEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);
				string eventKey = eventToAdd.GetEventGroupingKey();
				collectionToAdd.AddEventInfo(eventKey, eventToAdd);
			}

			private MethodDeclarationSyntax GetMethodNode(IMethodSymbol methodSymbol, CancellationToken cancellationToken) =>
				methodSymbol?.DeclaringSyntaxReferences.Length == 1
					? methodSymbol.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax
					: null;

			private GraphEventsCollection<GraphEventInfo> GetEventsCollectionToAdd(EventType eventType)		
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

					default:
						return null;
				}
			}

			private GraphEventsCollection<GraphFieldEventInfo> GetFieldEventsCollectionToAdd(EventType eventType)
			{
				switch (eventType)
				{
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

					case EventType.CommandPreparing:
						return CommandPreparingEvents;

					case EventType.ExceptionHandling:
						return ExceptionHandlingEvents;

					default:
						return null;
				}
			}
		}
	}
}
