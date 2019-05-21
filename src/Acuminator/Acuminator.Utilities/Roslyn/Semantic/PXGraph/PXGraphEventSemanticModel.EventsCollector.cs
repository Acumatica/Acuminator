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

			private readonly Dictionary<EventType, GraphEventsCollection<GraphRowEventInfo>> _rowEvents = 
				new Dictionary<EventType, GraphEventsCollection<GraphRowEventInfo>>
				{
					[EventType.RowSelecting] = new GraphEventsCollection<GraphRowEventInfo>(),
					[EventType.RowSelected] = new GraphEventsCollection<GraphRowEventInfo>(),

					[EventType.RowInserting] = new GraphEventsCollection<GraphRowEventInfo>(),
					[EventType.RowInserted] = new GraphEventsCollection<GraphRowEventInfo>(),

					[EventType.RowUpdating] = new GraphEventsCollection<GraphRowEventInfo>(),
					[EventType.RowUpdated] = new GraphEventsCollection<GraphRowEventInfo>(),

					[EventType.RowDeleting] = new GraphEventsCollection<GraphRowEventInfo>(),
					[EventType.RowDeleted] = new GraphEventsCollection<GraphRowEventInfo>(),

					[EventType.RowPersisting] = new GraphEventsCollection<GraphRowEventInfo>(),
					[EventType.RowPersisted] = new GraphEventsCollection<GraphRowEventInfo>(),
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

			public GraphEventsCollection<GraphRowEventInfo> GetRowEvents(EventType eventType) =>
				_rowEvents.TryGetValue(eventType, out GraphEventsCollection<GraphRowEventInfo> events)
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

				GraphRowEventInfo eventToAdd = new GraphRowEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);
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
