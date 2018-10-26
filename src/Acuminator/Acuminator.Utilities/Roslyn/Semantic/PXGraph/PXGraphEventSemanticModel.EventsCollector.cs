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



			public EventsCollector(PXGraphEventSemanticModel graphEventSemanticModel)
			{
				_graphEventSemanticModel = graphEventSemanticModel;
			}
		}
	}
}
