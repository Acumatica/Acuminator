using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class PXGraphSemanticModel
	{
		private readonly CancellationToken _cancellation;
		private readonly PXContext _pxContext;

		public bool IsProcessing { get; private set; }

		public GraphType Type { get; }

		public INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// The graph symbol. For the graph is the same as <see cref="Symbol"/>. For graph extensions is the extension's base graph.
		/// </summary>
		public INamedTypeSymbol GraphSymbol { get; }

		public ImmutableArray<StaticConstructorInfo> StaticConstructors { get; }
		public ImmutableArray<GraphInitializerInfo> Initializers { get; private set; }

		public ImmutableDictionary<string, DataViewInfo> ViewsByNames { get; }
		public IEnumerable<DataViewInfo> Views => ViewsByNames.Values;

		public ImmutableDictionary<string, DataViewDelegateInfo> ViewDelegatesByNames { get; }
		public IEnumerable<DataViewDelegateInfo> ViewDelegates => ViewDelegatesByNames.Values;


		public ImmutableDictionary<string, ActionInfo> ActionsByNames { get; }
		public IEnumerable<ActionInfo> Actions => ActionsByNames.Values;

		public ImmutableDictionary<string, ActionHandlerInfo> ActionHandlersByNames { get; }
		public IEnumerable<ActionHandlerInfo> ActionHandlers => ActionHandlersByNames.Values;

		private PXGraphSemanticModel(PXContext pxContext, GraphType type, INamedTypeSymbol symbol,
									 CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();

			_pxContext = pxContext;
			Type = type;
			Symbol = symbol;
			_cancellation = cancellation;

			switch (Type)
			{
				case GraphType.PXGraph:
					GraphSymbol = Symbol;
					break;
				case GraphType.PXGraphExtension:
					GraphSymbol = Symbol.GetGraphFromGraphExtension(_pxContext);
					break;
				case GraphType.None:
				default:
					GraphSymbol = null;
					break;
			}

			StaticConstructors = Symbol.GetStaticConstructors(_cancellation);
			ViewsByNames = GetDataViews();
			ViewDelegatesByNames = GetDataViewDelegates();

			ActionsByNames = GetActions();
			ActionHandlersByNames = GetActionHandlers();

			InitProcessingDelegatesInfo();
			InitDeclaredInitializers();
		}

		private void InitProcessingDelegatesInfo()
		{
			var processingViewSymbols = Views.Where(v => v.IsProcessing)
											 .Select(v => v.Symbol)
											 .ToImmutableHashSet();
			IsProcessing = processingViewSymbols.Count > 0;

			if (!IsProcessing)
			{
				return;
			}

			var declaringNodes = Symbol.DeclaringSyntaxReferences
									   .Select(r => r.GetSyntax(_cancellation));
			var walker = new ProcessingDelegatesWalker(_pxContext, processingViewSymbols, _cancellation);

			foreach (var node in declaringNodes)
			{
				walker.Visit(node);
			}

			foreach (var delegateInfo in walker.ParametersDelegateListByView)
			{
				ViewsByNames[delegateInfo.Key].ParametersDelegates = delegateInfo.Value.ToImmutableArray();
			}

			foreach (var delegateInfo in walker.ProcessDelegateListByView)
			{
				ViewsByNames[delegateInfo.Key].ProcessDelegates = delegateInfo.Value.ToImmutableArray();
			}

			foreach (var delegateInfo in walker.FinallyProcessDelegateListByView)
			{
				ViewsByNames[delegateInfo.Key].FinallyProcessDelegates = delegateInfo.Value.ToImmutableArray();
			}
		}

		private ImmutableDictionary<string, DataViewInfo> GetDataViews()
		{
			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, DataViewInfo>(StringComparer.OrdinalIgnoreCase);

			var rawViewInfos = Type == GraphType.PXGraph
				? Symbol.GetViewsWithSymbolsFromPXGraph(_pxContext)
				: Symbol.GetViewsFromGraphExtensionAndBaseGraph(_pxContext);

			return rawViewInfos.ToLookup(a => a.Item.ViewSymbol.Name, StringComparer.OrdinalIgnoreCase)
							   .ToImmutableDictionary(group => group.Key,
													  group => CreateViewInfo(group.First()),
													  keyComparer: StringComparer.OrdinalIgnoreCase);


			DataViewInfo CreateViewInfo(GraphOverridableItem<(ISymbol ViewSymbol, INamedTypeSymbol ViewType)> item)
			{
				var (viewSymbol, viewType) = item.Item;

				DataViewInfo baseViewInfo = item.Base != null
					? CreateViewInfo(item.Base)
					: null;

				return baseViewInfo == null
					? new DataViewInfo(viewSymbol, viewType, _pxContext, item.DeclarationOrder)
					: new DataViewInfo(viewSymbol, viewType, _pxContext, item.DeclarationOrder, baseViewInfo);
			}
		}

		private ImmutableDictionary<string, DataViewDelegateInfo> GetDataViewDelegates()
		{
			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, DataViewDelegateInfo>(StringComparer.OrdinalIgnoreCase);

			var rawDelegateInfos = Type == GraphType.PXGraph
				? Symbol.GetViewDelegatesFromGraph(ViewsByNames, _pxContext, _cancellation)
				: Symbol.GetViewDelegatesFromGraphExtensionAndBaseGraph(ViewsByNames, _pxContext, _cancellation);

			return rawDelegateInfos.ToLookup(d => d.Item.Symbol.Name, StringComparer.OrdinalIgnoreCase)
								   .ToImmutableDictionary(group => group.Key,
														  group => CreateViewDelegateInfo(group.First()),
														  keyComparer: StringComparer.OrdinalIgnoreCase);


			DataViewDelegateInfo CreateViewDelegateInfo(GraphOverridableItem<(MethodDeclarationSyntax, IMethodSymbol)> item)
			{
				var (node, method) = item.Item;

				DataViewDelegateInfo baseDelegateInfo = item.Base != null
					? CreateViewDelegateInfo(item.Base)
					: null;

				return baseDelegateInfo == null
					? new DataViewDelegateInfo(node, method, item.DeclarationOrder)
					: new DataViewDelegateInfo(node, method, item.DeclarationOrder, baseDelegateInfo);
			}
		}

		private ImmutableDictionary<string, ActionInfo> GetActions()
		{
			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, ActionInfo>(StringComparer.OrdinalIgnoreCase);

			var systemActionsRegister = new PXSystemActions.PXSystemActionsRegister(_pxContext);
			var rawActionInfos = Type == GraphType.PXGraph
				? Symbol.GetActionSymbolsWithTypesFromGraph(_pxContext)
				: Symbol.GetActionsFromGraphExtensionAndBaseGraph(_pxContext);

			return rawActionInfos.ToLookup(a => a.Item.ActionSymbol.Name, StringComparer.OrdinalIgnoreCase)
								 .ToImmutableDictionary(group => group.Key,
														group => CreateActionInfo(group.First()),
														keyComparer: StringComparer.OrdinalIgnoreCase);



			ActionInfo CreateActionInfo(GraphOverridableItem<(ISymbol ActionSymbol, INamedTypeSymbol ActionType)> item)
			{
				var (actionSymbol, actionType) = item.Item;

				ActionInfo baseActionInfo = item.Base != null
					? CreateActionInfo(item.Base)
					: null;

				return baseActionInfo == null
					? new ActionInfo(actionSymbol, actionType, item.DeclarationOrder, systemActionsRegister.IsSystemAction(actionType))
					: new ActionInfo(actionSymbol, actionType, item.DeclarationOrder,
									 systemActionsRegister.IsSystemAction(actionType), baseActionInfo);
			}
		}

		private ImmutableDictionary<string, ActionHandlerInfo> GetActionHandlers()
		{
			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, ActionHandlerInfo>(StringComparer.OrdinalIgnoreCase);

			var rawActionHandlerInfos = Type == GraphType.PXGraph
				? Symbol.GetActionHandlersFromGraph(ActionsByNames, _pxContext, _cancellation)
				: Symbol.GetActionHandlersFromGraphExtensionAndBaseGraph(ActionsByNames, _pxContext, _cancellation);

			return rawActionHandlerInfos.ToLookup(handler => handler.Item.Symbol.Name, StringComparer.OrdinalIgnoreCase)
										.ToImmutableDictionary(group => group.Key,
															   group => CreateActionHandlerInfo(group.First()),
															   keyComparer: StringComparer.OrdinalIgnoreCase);

			ActionHandlerInfo CreateActionHandlerInfo(GraphOverridableItem<(MethodDeclarationSyntax, IMethodSymbol)> item)
			{
				var (node, method) = item.Item;

				ActionHandlerInfo baseActionHandlerInfo = item.Base != null
					? CreateActionHandlerInfo(item.Base)
					: null;

				return baseActionHandlerInfo == null
					? new ActionHandlerInfo(node, method, item.DeclarationOrder)
					: new ActionHandlerInfo(node, method, item.DeclarationOrder, baseActionHandlerInfo);
			}
		}

		private void InitDeclaredInitializers()
		{
			_cancellation.ThrowIfCancellationRequested();

			List<GraphInitializerInfo> initializers = new List<GraphInitializerInfo>();

			if (Type == GraphType.PXGraph)
			{
				IEnumerable<GraphInitializerInfo> ctrs =
					Symbol.GetDeclaredInstanceConstructors(_cancellation)
						  .Select((ctr, order) => new GraphInitializerInfo(GraphInitializerType.InstanceCtr, ctr.Node, ctr.Symbol, order));

				initializers.AddRange(ctrs);
			}
			else if (Type == GraphType.PXGraphExtension)
			{
				(MethodDeclarationSyntax node, IMethodSymbol symbol) = Symbol.GetGraphExtensionInitialization(_pxContext, _cancellation);

				if (node != null && symbol != null)
				{
					initializers.Add(
						new GraphInitializerInfo(GraphInitializerType.InitializeMethod, node, symbol, declarationOrder: 0));
				}
			}

			Initializers = initializers.ToImmutableArray();
		}

		/// <summary>
		/// Returns one or multiple semantic models of PXGraph and PXGraphExtension descendants which are inferred from <paramref name="typeSymbol"/>
		/// </summary>
		/// <param name="pxContext">Context instance</param>
		/// <param name="typeSymbol">Symbol which is PXGraph or PXGraphExtension descendant and/or which uses PXGraph.InstanceCreated AddHandler method</param>
		/// <param name="semanticModel">Semantic model</param>
		/// <param name="cancellation">Cancellation</param>
		/// <returns></returns>
		public static IEnumerable<PXGraphSemanticModel> InferModels(PXContext pxContext, INamedTypeSymbol typeSymbol,
																	CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();
			pxContext.ThrowOnNull(nameof(pxContext));
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

			List<PXGraphSemanticModel> models = new List<PXGraphSemanticModel>();

			InferExplicitModel(pxContext, typeSymbol, models, cancellation);
			InferImplicitModels(pxContext, typeSymbol, models, cancellation);

			return models;
		}

		private static void InferImplicitModels(PXContext pxContext, INamedTypeSymbol typeSymbol,
												List<PXGraphSemanticModel> models, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			IEnumerable<InitDelegateInfo> delegates = GetInitDelegates(pxContext, typeSymbol, cancellation);

			foreach (InitDelegateInfo d in delegates)
			{
				GraphInitializerInfo info = new GraphInitializerInfo(GraphInitializerType.InstanceCreatedDelegate, d.Node,
																	 d.Symbol, d.DeclarationOrder);
				PXGraphSemanticModel existingModel = models.FirstOrDefault(m => m.Symbol.Equals(d.GraphTypeSymbol));
				PXGraphSemanticModel implicitModel;

				if (existingModel != null)
				{
					implicitModel = existingModel;
				}
				else
				{
					implicitModel = new PXGraphSemanticModel(pxContext, d.GraphType, d.GraphTypeSymbol, cancellation);
					models.Add(implicitModel);
				}

				implicitModel.Initializers = implicitModel.Initializers.Add(info);
			}
		}

		private static void InferExplicitModel(PXContext pxContext, INamedTypeSymbol typeSymbol,
											   List<PXGraphSemanticModel> models, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			GraphType graphType = GraphType.None;

			if (typeSymbol.IsPXGraph(pxContext))
			{
				graphType = GraphType.PXGraph;
			}
			else if (typeSymbol.IsPXGraphExtension(pxContext))
			{
				graphType = GraphType.PXGraphExtension;
			}

			if (graphType != GraphType.None)
			{
				PXGraphSemanticModel explicitModel = new PXGraphSemanticModel(pxContext, graphType, typeSymbol, cancellation);

				models.Add(explicitModel);
			}
		}

		private static IEnumerable<InitDelegateInfo> GetInitDelegates(PXContext pxContext, INamedTypeSymbol typeSymbol,
																	  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			IEnumerable<SyntaxNode> declaringNodes = typeSymbol.DeclaringSyntaxReferences
															   .Select(r => r.GetSyntax(cancellation));
			InstanceCreatedEventsAddHandlerWalker walker = new InstanceCreatedEventsAddHandlerWalker(pxContext, cancellation);

			foreach (SyntaxNode node in declaringNodes)
			{
				cancellation.ThrowIfCancellationRequested();
				walker.Visit(node);
			}

			return walker.GraphInitDelegates;
		}
	}
}
