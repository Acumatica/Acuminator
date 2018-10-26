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

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> CacheAttachedEvents { get; } =
				new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowSelectingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowSelectedEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowInsertingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowInsertedEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowUpdatingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowUpdatedEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowDeletingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowDeletedEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowPersistingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> RowPersistedEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> FieldSelectingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> FieldDefaultingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> FieldVerifyingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> FieldUpdatingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> FieldUpdatedEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> CommandPreparingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			public GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)> ExceptionHandlingEvents { get; } =
							new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();



			public EventsCollector(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
			{
				_pxContext = context;
				_graphEventSemanticModel = graphEventSemanticModel;
			}

			public void AddEvent(EventHandlerSignatureType signatureType, EventType eventType, IMethodSymbol methodSymbol)
			{
				switch (eventType)
				{
					case EventType.CacheAttached:
						break;
					case EventType.RowSelecting:
						break;
					case EventType.RowSelected:
						break;
					case EventType.RowInserting:
						break;
					case EventType.RowInserted:
						break;
					case EventType.RowUpdating:
						break;
					case EventType.RowUpdated:
						break;
					case EventType.RowDeleting:
						break;
					case EventType.RowDeleted:
						break;
					case EventType.RowPersisting:
						break;
					case EventType.RowPersisted:
						break;
					case EventType.FieldSelecting:
						break;
					case EventType.FieldDefaulting:
						break;
					case EventType.FieldVerifying:
						break;
					case EventType.FieldUpdating:
						break;
					case EventType.FieldUpdated:
						break;
					case EventType.CommandPreparing:
						break;
					case EventType.ExceptionHandling:
						break;				
				}
			}
		}
	}
}
