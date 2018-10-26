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

			public GraphOverridableItemsCollection<GraphEventInfo> CacheAttachedEvents { get; } =
				new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowSelectingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowSelectedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowInsertingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowInsertedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowUpdatingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowUpdatedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowDeletingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowDeletedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowPersistingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> RowPersistedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> FieldSelectingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> FieldDefaultingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> FieldVerifyingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> FieldUpdatingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> FieldUpdatedEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> CommandPreparingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();

			public GraphOverridableItemsCollection<GraphEventInfo> ExceptionHandlingEvents { get; } =
							new GraphOverridableItemsCollection<GraphEventInfo>();



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

				GraphOverridableItemsCollection<GraphEventInfo> colectionToAdd = null;

				switch (eventType)
				{
					case EventType.CacheAttached:
						colectionToAdd = CacheAttachedEvents;
						break;
					case EventType.RowSelecting:
						colectionToAdd = RowSelectingEvents;
						break;
					case EventType.RowSelected:
						colectionToAdd = RowSelectedEvents;
						break;
					case EventType.RowInserting:
						colectionToAdd = RowInsertingEvents;
						break;
					case EventType.RowInserted:
						colectionToAdd = RowInsertedEvents;
						break;
					case EventType.RowUpdating:
						colectionToAdd = RowUpdatingEvents;
						break;
					case EventType.RowUpdated:
						colectionToAdd = RowUpdatedEvents;
						break;
					case EventType.RowDeleting:
						colectionToAdd = RowDeletingEvents;
						break;
					case EventType.RowDeleted:
						colectionToAdd = RowDeletedEvents;
						break;
					case EventType.RowPersisting:
						colectionToAdd = RowPersistingEvents;
						break;
					case EventType.RowPersisted:
						colectionToAdd = RowPersistedEvents;
						break;
					case EventType.FieldSelecting:
						colectionToAdd = FieldSelectingEvents;
						break;
					case EventType.FieldDefaulting:
						colectionToAdd = FieldDefaultingEvents;
						break;
					case EventType.FieldVerifying:
						colectionToAdd = FieldVerifyingEvents;
						break;
					case EventType.FieldUpdating:
						colectionToAdd = FieldUpdatingEvents;
						break;
					case EventType.FieldUpdated:
						colectionToAdd = FieldUpdatedEvents;
						break;		
				}

				if (colectionToAdd == null)
					return;

				colectionToAdd.Add(methodSymbol.Name, new GraphEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType),
								   declarationOrder);
			}
		}
	}
}
