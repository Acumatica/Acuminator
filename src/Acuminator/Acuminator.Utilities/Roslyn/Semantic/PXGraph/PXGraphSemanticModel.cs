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
            var processingViewSymbols = Views
                                        .Where(v => v.IsProcessing)
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
            var viewByNameDictionary = new Dictionary<string, DataViewInfo>(StringComparer.OrdinalIgnoreCase);

            if (Type == GraphType.PXGraph)
            {
                var graphViews = Symbol.GetViewsWithSymbolsFromPXGraph(_pxContext).Select(item => item.Item);

                foreach (var (ViewSymbol, ViewType) in graphViews)
                {
                    viewByNameDictionary[ViewSymbol.Name] = new DataViewInfo(ViewSymbol, ViewType, _pxContext);
                }
            }
            else
            {
                var extViews = Symbol.GetViewsFromGraphExtensionAndBaseGraph(_pxContext);

                foreach (var view in extViews)
                {
                    var viewInfo = view.Base == null
                        ? new DataViewInfo(view.Item.ViewSymbol,
                                           view.Item.ViewType,
                                           _pxContext)
                        : new DataViewInfo(view.Item.ViewSymbol,
                                           view.Item.ViewType,
                                           _pxContext,
                                           new DataViewInfo(view.Base.Item.ViewSymbol, view.Base.Item.ViewType, _pxContext));

                    viewByNameDictionary[view.Item.ViewSymbol.Name] = viewInfo;
                }
            }

            return viewByNameDictionary.ToImmutableDictionary();
        }

        private ImmutableDictionary<string, DataViewDelegateInfo> GetDataViewDelegates()
        {
            var viewDelegateByNameDictionary = new Dictionary<string, DataViewDelegateInfo>(StringComparer.OrdinalIgnoreCase);
            var viewSymbols = Views.Select(v => v.Symbol);

			if (Type == GraphType.PXGraph)
			{
				var graphDelegates = Symbol.GetViewDelegatesFromGraph(viewSymbols, _pxContext, _cancellation);

                foreach (var del in graphDelegates)
                {
                    viewDelegateByNameDictionary[del.Symbol.Name] = new DataViewDelegateInfo(del.Node, del.Symbol);
                }
            }
            else
            {
                var extDelegates = Symbol.GetViewDelegatesFromGraphExtensionAndBaseGraph(viewSymbols, _pxContext, _cancellation);

                foreach (var del in extDelegates)
                {
                    var delegateInfo = del.Base == null
                        ? new DataViewDelegateInfo(del.Item.Node, del.Item.Symbol)
                        : new DataViewDelegateInfo(del.Item.Node, del.Item.Symbol, new DataViewDelegateInfo(del.Base.Item.Node, del.Base.Item.Symbol));

                    viewDelegateByNameDictionary[del.Item.Symbol.Name] = delegateInfo;
                }
            }

            return viewDelegateByNameDictionary.ToImmutableDictionary();
        }

		private ImmutableDictionary<string, ActionInfo> GetActions()
		{
			var systemActionsRegister = new PXSystemActions.PXSystemActionsRegister(_pxContext);

			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, ActionInfo>(StringComparer.OrdinalIgnoreCase);

			var rawActionInfos = Type == GraphType.PXGraph
				? Symbol.GetActionSymbolsWithTypesFromGraph(_pxContext)
				: Symbol.GetActionsFromGraphExtensionAndBaseGraph(_pxContext);

			return rawActionInfos.ToLookup(a => a.Item.ActionSymbol.Name, StringComparer.OrdinalIgnoreCase)
								 .ToImmutableDictionary(group => group.Key,
														group => CreateActionInfo(group.First()),
														keyComparer: StringComparer.OrdinalIgnoreCase);

			//--------------------------------------------------------Local Function--------------------------------------------
			ActionInfo CreateActionInfo(GraphOverridableItem<(ISymbol ActionSymbol, INamedTypeSymbol ActionType)> item)
			{
				var (actionSymbol, actionType) = item.Item;

				ActionInfo baseActionInfo = item.Base != null
					? baseActionInfo = CreateActionInfo(item.Base)
					: null;

				return baseActionInfo == null
					? new ActionInfo(actionSymbol, actionType, systemActionsRegister.IsSystemAction(actionType))
					: new ActionInfo(actionSymbol, actionType, systemActionsRegister.IsSystemAction(actionType), baseActionInfo);
			}
		}

		private ImmutableDictionary<string, ActionHandlerInfo> GetActionHandlers()
		{
			switch (Type)
			{
				case GraphType.PXGraph:
					return Symbol.GetActionHandlersFromGraph(ActionsByNames, _pxContext, _cancellation)
								 .ToImmutableDictionary(a => a.Item.Symbol.Name, CreateActionHandlerInfo,
															 StringComparer.OrdinalIgnoreCase);
				case GraphType.PXGraphExtension:
					return Symbol.GetActionHandlersFromGraphExtensionAndBaseGraph(ActionsByNames, _pxContext, _cancellation)
								 .ToImmutableDictionary(a => a.Item.Symbol.Name, CreateActionHandlerInfo,
														StringComparer.OrdinalIgnoreCase);
				case GraphType.None:
				default:
					return ImmutableDictionary.Create<string, ActionHandlerInfo>(StringComparer.OrdinalIgnoreCase);
			}

			//--------------------------------------------------------Local Function--------------------------------------------
			ActionHandlerInfo CreateActionHandlerInfo(GraphOverridableItem<(MethodDeclarationSyntax, IMethodSymbol)> item)
			{
				var (node, method) = item.Item;

				ActionHandlerInfo baseActionHandlerInfo = item.Base != null
					? baseActionHandlerInfo = CreateActionHandlerInfo(item.Base)
					: null;

				return baseActionHandlerInfo == null
					? new ActionHandlerInfo(node, method)
					: new ActionHandlerInfo(node, method, baseActionHandlerInfo);
			}
		}

		private void InitDeclaredInitializers()
		{
			_cancellation.ThrowIfCancellationRequested();

			List<GraphInitializerInfo> initializers = new List<GraphInitializerInfo>();

			if (Type == GraphType.PXGraph)
			{
				IEnumerable<GraphInitializerInfo> ctrs = Symbol.GetDeclaredInstanceConstructors(_cancellation)
														 .Select(ctr => new GraphInitializerInfo(GraphInitializerType.InstanceCtr, ctr.Node, ctr.Symbol));
				initializers.AddRange(ctrs);
			}
			else if (Type == GraphType.PXGraphExtension)
			{
				(MethodDeclarationSyntax node, IMethodSymbol symbol) = Symbol.GetGraphExtensionInitialization(_pxContext, _cancellation);

				if (node != null && symbol != null)
				{
					initializers.Add(new GraphInitializerInfo(GraphInitializerType.InitializeMethod, node, symbol));
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
				GraphInitializerInfo info = new GraphInitializerInfo(GraphInitializerType.InstanceCreatedDelegate, d.Node, d.Symbol);
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
