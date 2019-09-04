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

			private readonly Dictionary<EventType, OverridableItemsCollection<GraphRowEventInfo>> _rowEvents = 
				new Dictionary<EventType, OverridableItemsCollection<GraphRowEventInfo>>
				{
					[EventType.RowSelecting] = new OverridableItemsCollection<GraphRowEventInfo>(),
					[EventType.RowSelected] = new OverridableItemsCollection<GraphRowEventInfo>(),

					[EventType.RowInserting] = new OverridableItemsCollection<GraphRowEventInfo>(),
					[EventType.RowInserted] = new OverridableItemsCollection<GraphRowEventInfo>(),

					[EventType.RowUpdating] = new OverridableItemsCollection<GraphRowEventInfo>(),
					[EventType.RowUpdated] = new OverridableItemsCollection<GraphRowEventInfo>(),

					[EventType.RowDeleting] = new OverridableItemsCollection<GraphRowEventInfo>(),
					[EventType.RowDeleted] = new OverridableItemsCollection<GraphRowEventInfo>(),

					[EventType.RowPersisting] = new OverridableItemsCollection<GraphRowEventInfo>(),
					[EventType.RowPersisted] = new OverridableItemsCollection<GraphRowEventInfo>(),
				};

			private readonly Dictionary<EventType, OverridableItemsCollection<GraphFieldEventInfo>> _fieldEvents =
				new Dictionary<EventType, OverridableItemsCollection<GraphFieldEventInfo>>
				{
					[EventType.FieldSelecting] = new OverridableItemsCollection<GraphFieldEventInfo>(),
					[EventType.FieldDefaulting] = new OverridableItemsCollection<GraphFieldEventInfo>(),
					[EventType.FieldVerifying] = new OverridableItemsCollection<GraphFieldEventInfo>(),
					[EventType.FieldUpdating] = new OverridableItemsCollection<GraphFieldEventInfo>(),
					[EventType.FieldUpdated] = new OverridableItemsCollection<GraphFieldEventInfo>(),

					[EventType.CacheAttached] = new OverridableItemsCollection<GraphFieldEventInfo>(),

					[EventType.CommandPreparing] = new OverridableItemsCollection<GraphFieldEventInfo>(),
					[EventType.ExceptionHandling] = new OverridableItemsCollection<GraphFieldEventInfo>(),				
				};

			public EventsCollector(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
			{
				_pxContext = context;
				_graphEventSemanticModel = graphEventSemanticModel;
			}

			public OverridableItemsCollection<GraphRowEventInfo> GetRowEvents(EventType eventType) =>
				_rowEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphRowEventInfo> events)
					? events
					: null;

			public OverridableItemsCollection<GraphFieldEventInfo> GetFieldEvents(EventType eventType) =>
				_fieldEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphFieldEventInfo> events)
					? events
					: null;

			public void AddEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
								 int declarationOrder, CancellationToken cancellationToken)
			{
				var methodNode = GetMethodNode(methodSymbol, cancellationToken);

				if (methodNode == null || !_rowEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphRowEventInfo> collectionToAdd))
					return;

				GraphRowEventInfo eventToAdd = new GraphRowEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);

				if (!eventToAdd.DacName.IsNullOrEmpty())
				{
					collectionToAdd.Add(eventToAdd);
				}
			}

			public void AddFieldEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
									  int declarationOrder, CancellationToken cancellationToken)
			{
				var methodNode = GetMethodNode(methodSymbol, cancellationToken);

				if (methodNode == null || !_fieldEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphFieldEventInfo> collectionToAdd))
					return;

				GraphFieldEventInfo eventToAdd = new GraphFieldEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);

				if (!eventToAdd.DacName.IsNullOrEmpty() && !eventToAdd.DacFieldName.IsNullOrEmpty())
				{
					collectionToAdd.Add(eventToAdd);
				}
			}

			private MethodDeclarationSyntax GetMethodNode(IMethodSymbol methodSymbol, CancellationToken cancellationToken) =>
				methodSymbol?.DeclaringSyntaxReferences.Length == 1
					? methodSymbol.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) as MethodDeclarationSyntax
					: null;			
		}
	}
}
