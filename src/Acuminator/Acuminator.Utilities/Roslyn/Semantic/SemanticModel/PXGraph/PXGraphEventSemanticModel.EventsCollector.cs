#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public partial class PXGraphEventSemanticModel : ISemanticModel
	{
		private class EventsCollector
		{
			private readonly PXContext _pxContext;
			private readonly PXGraphEventSemanticModel _graphEventSemanticModel;

			private readonly Dictionary<EventType, OverridableItemsCollection<GraphRowEventInfo>> _rowEvents = 
				new()
				{
					[EventType.RowSelecting]  = [],
					[EventType.RowSelected]   = [],

					[EventType.RowInserting]  = [],
					[EventType.RowInserted]   = [],

					[EventType.RowUpdating]   = [],
					[EventType.RowUpdated] 	  = [],

					[EventType.RowDeleting]   = [],
					[EventType.RowDeleted] 	  = [],

					[EventType.RowPersisting] = [],
					[EventType.RowPersisted]  = [],
				};

			private readonly Dictionary<EventType, OverridableItemsCollection<GraphFieldEventInfo>> _fieldEvents =
				new()
				{
					[EventType.FieldSelecting] 	  = [],
					[EventType.FieldDefaulting]   = [],
					[EventType.FieldVerifying] 	  = [],
					[EventType.FieldUpdating] 	  = [],
					[EventType.FieldUpdated] 	  = [],

					[EventType.CacheAttached] 	  = [],

					[EventType.CommandPreparing]  = [],
					[EventType.ExceptionHandling] = [],
				};

			public EventsCollector(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
			{
				_pxContext = context;
				_graphEventSemanticModel = graphEventSemanticModel;
			}

			public OverridableItemsCollection<GraphRowEventInfo>? GetRowEvents(EventType eventType) =>
				_rowEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphRowEventInfo> events)
					? events
					: null;

			public OverridableItemsCollection<GraphFieldEventInfo>? GetFieldEvents(EventType eventType) =>
				_fieldEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphFieldEventInfo> events)
					? events
					: null;

			public void AddEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
								 int declarationOrder, CancellationToken cancellationToken)
			{
				var methodNode = GetMethodNode(methodSymbol, cancellationToken);

				if (!_rowEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphRowEventInfo> collectionToAdd))
					return;

				var eventToAdd = new GraphRowEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);

				if (!eventToAdd.DacName.IsNullOrEmpty())
				{
					collectionToAdd.Add(eventToAdd);
				}
			}

			public void AddFieldEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol,
									  int declarationOrder, CancellationToken cancellationToken)
			{
				var methodNode = GetMethodNode(methodSymbol, cancellationToken);

				if (!_fieldEvents.TryGetValue(eventType, out OverridableItemsCollection<GraphFieldEventInfo> collectionToAdd))
					return;

				var eventToAdd = new GraphFieldEventInfo(methodNode, methodSymbol, declarationOrder, signatureType, eventType);

				if (!eventToAdd.DacName.IsNullOrEmpty() && !eventToAdd.DacFieldName.IsNullOrEmpty())
				{
					collectionToAdd.Add(eventToAdd);
				}
			}

			private MethodDeclarationSyntax? GetMethodNode(IMethodSymbol methodSymbol, CancellationToken cancellationToken) =>
				methodSymbol.GetSyntax(cancellationToken) as MethodDeclarationSyntax;
		}
	}
}
