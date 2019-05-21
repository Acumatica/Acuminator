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

			private readonly Dictionary<EventType, GraphEventsCollection<GraphEventInfo>> _rowEvents = 
				new Dictionary<EventType, GraphEventsCollection<GraphEventInfo>>
				{
					[EventType.RowSelecting] = new GraphEventsCollection<GraphEventInfo>(),
					[EventType.RowSelected] = new GraphEventsCollection<GraphEventInfo>(),

					[EventType.RowInserting] = new GraphEventsCollection<GraphEventInfo>(),
					[EventType.RowInserted] = new GraphEventsCollection<GraphEventInfo>(),

					[EventType.RowUpdating] = new GraphEventsCollection<GraphEventInfo>(),
					[EventType.RowUpdated] = new GraphEventsCollection<GraphEventInfo>(),

					[EventType.RowDeleting] = new GraphEventsCollection<GraphEventInfo>(),
					[EventType.RowDeleted] = new GraphEventsCollection<GraphEventInfo>(),

					[EventType.RowPersisting] = new GraphEventsCollection<GraphEventInfo>(),
					[EventType.RowPersisted] = new GraphEventsCollection<GraphEventInfo>(),
				};

			private readonly Dictionary<EventType, GraphEventsCollection<GraphFieldEventInfo>> _fieldEvents =
				new Dictionary<EventType, GraphEventsCollection<GraphFieldEventInfo>>
				{
					[EventType.FieldSelecting] = new GraphEventsCollection<GraphFieldEventInfo>(),
					[EventType.FieldDefaulting] = new GraphEventsCollection<GraphFieldEventInfo>(),
					[EventType.FieldVerifying] = new GraphEventsCollection<GraphFieldEventInfo>(),
					[EventType.FieldUpdating] = new GraphEventsCollection<GraphFieldEventInfo>(),
					[EventType.FieldUpdated] = new GraphEventsCollection<GraphFieldEventInfo>(),

					[EventType.CacheAttached] = new GraphEventsCollection<GraphFieldEventInfo>(),

					[EventType.CommandPreparing] = new GraphEventsCollection<GraphFieldEventInfo>(),
					[EventType.ExceptionHandling] = new GraphEventsCollection<GraphFieldEventInfo>(),				
				};

			public EventsCollector(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
			{
				_pxContext = context;
				_graphEventSemanticModel = graphEventSemanticModel;
			}

			public GraphEventsCollection<GraphEventInfo> GetRowEvents(EventType eventType) =>
				_rowEvents.TryGetValue(eventType, out GraphEventsCollection<GraphEventInfo> events)
					? events
					: null;

			public GraphEventsCollection<GraphFieldEventInfo> GetFieldEvents(EventType eventType) =>
				_fieldEvents.TryGetValue(eventType, out GraphEventsCollection<GraphFieldEventInfo> events)
					? events
					: null;

			public void AddEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
								 int declarationOrder, CancellationToken cancellationToken)
			{
				var methodNode = GetMethodNode(methodSymbol, cancellationToken);

				if (methodNode == null || !_rowEvents.TryGetValue(eventType, out var collectionToAdd))
					return;

				GraphEventInfo eventToAdd = new GraphEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);
				string eventKey = eventToAdd.GetEventGroupingKey();
				collectionToAdd.AddEventInfo(eventKey, eventToAdd);
			}

			public void AddFieldEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
									  int declarationOrder, CancellationToken cancellationToken)
			{
				var methodNode = GetMethodNode(methodSymbol, cancellationToken);

				if (methodNode == null || !_fieldEvents.TryGetValue(eventType, out var collectionToAdd))
					return;

				GraphFieldEventInfo eventToAdd = new GraphFieldEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);
				string eventKey = eventToAdd.GetEventGroupingKey();
				collectionToAdd.AddEventInfo(eventKey, eventToAdd);
			}

			private MethodDeclarationSyntax GetMethodNode(IMethodSymbol methodSymbol, CancellationToken cancellationToken) =>
				methodSymbol?.DeclaringSyntaxReferences.Length == 1
					? methodSymbol.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax
					: null;			
		}
	}
}
