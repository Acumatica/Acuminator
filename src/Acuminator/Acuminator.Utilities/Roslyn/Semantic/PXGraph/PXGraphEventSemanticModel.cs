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


		public ImmutableDictionary<string, CacheAttachedInfo> CacheAttachedByDacName { get; }

		public IEnumerable<CacheAttachedInfo> CacheAttachedEvents => CacheAttachedByDacName.Values;

		private PXGraphEventSemanticModel(PXContext pxContext, PXGraphSemanticModel baseGraphModel,
									      CancellationToken cancellation = default)
		{
			_pxContext = pxContext;
			_cancellation = cancellation;
			BaseGraphModel = baseGraphModel;

			if (BaseGraphModel.Type != GraphType.None)
			{

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

		private void InitializeEvents()
		{
			_cancellation.ThrowIfCancellationRequested();
			var methods = GetAllGraphMethodsFromBaseToDerived();

			var cacheAttachedCollection = new GraphOverridableItemsCollection<(MethodDeclarationSyntax Node, IMethodSymbol Symbol)>();

			foreach (IMethodSymbol method in methods)
			{
				_cancellation.ThrowIfCancellationRequested();

				var (eventType, eventSignatureType) = method.GetEventHandlerInfo(_pxContext);

				if (eventSignatureType == EventHandlerSignatureType.None)
					continue;

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
					case EventType.None:
					default:
						continue;
				}
			}

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

		private void ProcessEvent(IMethodSymbol eventSymbol, EventType eventType, EventHandlerSignatureType signatureType)
		{

		}
	}
}
