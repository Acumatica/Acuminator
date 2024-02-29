﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class PXGraphSemanticModel : ISemanticModel
	{
		private readonly CancellationToken _cancellation;

		internal PXContext PXContext { get; }

		public GraphSemanticModelCreationOptions ModelCreationOptions { get; }

		public bool IsProcessing { get; private set; }

		public GraphType Type { get; }

		public INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// The graph symbol. For the graph is the same as <see cref="Symbol"/>. For graph extensions is the extension's base graph.
		/// </summary>
		public ITypeSymbol? GraphSymbol { get; }

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

		public ImmutableArray<PXOverrideInfo> PXOverrides { get; }

		/// <summary>
		/// Actions which are declared in a graph/graph extension represented by this semantic model instance.
		/// </summary>
		public IEnumerable<ActionInfo> DeclaredActions => Type == GraphType.None
			? Enumerable.Empty<ActionInfo>()
			: Actions.Where(action => action.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Action handlers which are declared in a graph/graph extension represented by this semantic model instance.
		/// </summary>
		public IEnumerable<ActionHandlerInfo> DeclaredActionHandlers => Type == GraphType.None
			? Enumerable.Empty<ActionHandlerInfo>()
			: ActionHandlers.Where(handler => handler.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Views which are declared in a graph/graph extension represented by this semantic model instance.
		/// </summary>
		public IEnumerable<DataViewInfo> DeclaredViews => Type == GraphType.None
			? Enumerable.Empty<DataViewInfo>()
			: Views.Where(view => view.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// View delegates which are declared in a graph/graph extension represented by this semantic model instance.
		/// </summary>
		public IEnumerable<DataViewDelegateInfo> DeclaredViewDelegates => Type == GraphType.None
			? Enumerable.Empty<DataViewDelegateInfo>()
			: ViewDelegates.Where(viewDelegate => viewDelegate.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Gets the info about IsActive method for graph extensions. Can be <c>null</c>. Always <c>null</c> for graphs.
		/// </summary>
		/// <value>
		/// The info about IsActive method.
		/// </value>
		public IsActiveMethodInfo? IsActiveMethodInfo { get; }

		/// <summary>
		/// Gets the info about IsActiveForGraph&lt;TGraph&gt; method for graph extensions. Can be <c>null</c>. Always <c>null</c> for graphs.
		/// </summary>
		/// <value>
		/// The info about IsActiveForGraph&lt;TGraph&gt; method.
		/// </value>
		public IsActiveForGraphMethodInfo? IsActiveForGraphMethodInfo { get; }

		/// <summary>
		/// Gets infos about the Configure method overrides declared in the graph or graph extension's class hierarchy. 
		/// </summary>
		/// <value>
		/// Infos about Configure method overrides.
		/// </value>
		public ImmutableArray<ConfigureMethodInfo> ConfigureMethodOverrides { get; }

		/// <summary>
		/// Flag indicating whether the graph or graph extension configures workflow.
		/// </summary>
		public bool ConfiguresWorkflow => !ConfigureMethodOverrides.IsDefaultOrEmpty;

		private PXGraphSemanticModel(PXContext pxContext, GraphType type, INamedTypeSymbol symbol, GraphSemanticModelCreationOptions modelCreationOptions,
									 CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();

			PXContext 			 = pxContext;
			Type 				 = type;
			Symbol 				 = symbol;
			_cancellation 		 = cancellation;
			ModelCreationOptions = modelCreationOptions;

			GraphSymbol = Type switch
			{
				GraphType.PXGraph 		   => Symbol,
				GraphType.PXGraphExtension => Symbol.GetGraphFromGraphExtension(PXContext),
				_ 						   => null,
			};

			StaticConstructors 	 = Symbol.GetStaticConstructors(_cancellation);
			ViewsByNames 		 = GetDataViews();
			ViewDelegatesByNames = GetDataViewDelegates();

			ActionsByNames 		  = GetActions();
			ActionHandlersByNames = GetActionHandlers();

			InitProcessingDelegatesInfo();

			Initializers 			   = GetDeclaredInitializers().ToImmutableArray();
			IsActiveMethodInfo 		   = GetIsActiveMethodInfo();
			IsActiveForGraphMethodInfo = GetIsActiveForGraphMethodInfo();
			ConfigureMethodOverrides   = GetConfigureMethodOverrides();
			PXOverrides 			   = GetDeclaredPXOverrideInfos();
		}

		private void InitProcessingDelegatesInfo()
		{
			if (!ModelCreationOptions.HasFlag(GraphSemanticModelCreationOptions.CollectProcessingDelegates))
			{
				IsProcessing = Views.Any(v => v.IsProcessing);
				return;
			}

			var processingViewSymbols = Views.Where(v => v.IsProcessing)
											 .Select(v => v.Symbol)
											 .ToImmutableHashSet();
			IsProcessing = processingViewSymbols.Count > 0;

			if (!IsProcessing)
			{
				return;
			}

			_cancellation.ThrowIfCancellationRequested();
			var declaringNodes = Symbol.DeclaringSyntaxReferences
									   .Select(r => r.GetSyntax(_cancellation));
			var walker = new ProcessingDelegatesWalker(PXContext, processingViewSymbols, _cancellation);

			foreach (var node in declaringNodes)
			{
				walker.Visit(node);
			}

			foreach (var (viewName, paramsDelegateInfo) in walker.ParametersDelegateListByView)
			{
				ViewsByNames[viewName].ParametersDelegates = paramsDelegateInfo.ToImmutableArray();
			}

			_cancellation.ThrowIfCancellationRequested();

			foreach (var (viewName, processDelegateInfo) in walker.ProcessDelegateListByView)
			{
				ViewsByNames[viewName].ProcessDelegates = processDelegateInfo.ToImmutableArray();
			}

			_cancellation.ThrowIfCancellationRequested();

			foreach (var (viewName, finalProcessDelegateInfo) in walker.FinallyProcessDelegateListByView)
			{
				ViewsByNames[viewName].FinallyProcessDelegates = finalProcessDelegateInfo.ToImmutableArray();
			}
		}

		private ImmutableDictionary<string, DataViewInfo> GetDataViews() =>
			GetInfos(() => Symbol.GetViewsWithSymbolsFromPXGraph(PXContext),
					 () => Symbol.GetViewsFromGraphExtensionAndBaseGraph(PXContext));

		private ImmutableDictionary<string, DataViewDelegateInfo> GetDataViewDelegates() =>
			GetInfos(() => Symbol.GetViewDelegatesFromGraph(ViewsByNames, PXContext, cancellation: _cancellation),
					 () => Symbol.GetViewDelegatesFromGraphExtensionAndBaseGraph(ViewsByNames, PXContext, _cancellation));

		private ImmutableDictionary<string, ActionInfo> GetActions() =>
			GetInfos(() => Symbol.GetActionSymbolsWithTypesFromGraph(PXContext),
					 () => Symbol.GetActionsFromGraphExtensionAndBaseGraph(PXContext));

		private ImmutableDictionary<string, ActionHandlerInfo> GetActionHandlers() =>
			GetInfos(() => Symbol.GetActionHandlersFromGraph(ActionsByNames, PXContext, cancellation: _cancellation),
					 () => Symbol.GetActionHandlersFromGraphExtensionAndBaseGraph(ActionsByNames, PXContext, _cancellation));

		private ImmutableDictionary<string, TInfo> GetInfos<TInfo>(Func<OverridableItemsCollection<TInfo>> graphInfosSelector,
																   Func<OverridableItemsCollection<TInfo>> graphExtInfosSelector)
		where TInfo : IOverridableItem<TInfo>
		{
			if (Type == GraphType.None)
				return ImmutableDictionary.Create<string, TInfo>(StringComparer.OrdinalIgnoreCase);

			var infos = Type == GraphType.PXGraph
				? graphInfosSelector()
				: graphExtInfosSelector();

			return infos.ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);
		}

		private IEnumerable<GraphInitializerInfo> GetDeclaredInitializers()
		{
			_cancellation.ThrowIfCancellationRequested();

			if (Type == GraphType.PXGraph)
			{
				return Symbol.GetDeclaredInstanceConstructors(_cancellation)
							 .Select((ctr, order) => new GraphInitializerInfo(GraphInitializerType.InstanceConstructor, ctr.Node, ctr.Symbol, order));
			}
			else if (Type == GraphType.PXGraphExtension)
			{
				(MethodDeclarationSyntax node, IMethodSymbol symbol) = Symbol.GetGraphExtensionInitialization(PXContext, _cancellation);

				if (node != null && symbol != null)
				{
					return new GraphInitializerInfo(GraphInitializerType.InitializeMethod, node, symbol, declarationOrder: 0)
								.ToEnumerable();
				}
			}

			return Enumerable.Empty<GraphInitializerInfo>();
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
																	GraphSemanticModelCreationOptions modelCreationOptions,
																	CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();
			pxContext.ThrowOnNull(nameof(pxContext));
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

			var models = new List<PXGraphSemanticModel>(capacity: 1);
			var explicitModel = InferExplicitModel(pxContext, typeSymbol, modelCreationOptions, cancellation);

			if (explicitModel != null)
			{
				models.Add(explicitModel);
			}

			if (modelCreationOptions.HasFlag(GraphSemanticModelCreationOptions.InferImplicitModels))
			{
				InferImplicitModels(pxContext, typeSymbol, modelCreationOptions, models, cancellation);
			}

			return models;
		}

		private static void InferImplicitModels(PXContext pxContext, INamedTypeSymbol typeSymbol,
												GraphSemanticModelCreationOptions modelCreationOptions,
												List<PXGraphSemanticModel> models, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			IEnumerable<InitDelegateInfo> delegates = GetInitDelegates(pxContext, typeSymbol, cancellation);

			foreach (InitDelegateInfo initDelegateInfo in delegates)
			{
				GraphInitializerInfo info = new GraphInitializerInfo(GraphInitializerType.InstanceCreatedDelegate, initDelegateInfo.Node,
																	 initDelegateInfo.Symbol, initDelegateInfo.DeclarationOrder);
				PXGraphSemanticModel existingModel = models.FirstOrDefault(m => m.Symbol.Equals(initDelegateInfo.GraphTypeSymbol));
				PXGraphSemanticModel implicitModel;

				if (existingModel != null)
				{
					implicitModel = existingModel;
				}
				else
				{
					implicitModel = new PXGraphSemanticModel(pxContext, initDelegateInfo.GraphType, initDelegateInfo.GraphTypeSymbol, modelCreationOptions, cancellation);
					models.Add(implicitModel);
				}

				implicitModel.Initializers = implicitModel.Initializers.Add(info);
			}
		}

		public static PXGraphSemanticModel? InferExplicitModel(PXContext pxContext, INamedTypeSymbol typeSymbol,
															  GraphSemanticModelCreationOptions modelCreationOptions,
															  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			pxContext.ThrowOnNull(nameof(pxContext));
			typeSymbol.ThrowOnNull(nameof(typeSymbol));

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
				return new PXGraphSemanticModel(pxContext, graphType, typeSymbol, modelCreationOptions, cancellation);
			}

			return null;
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

		private IsActiveMethodInfo? GetIsActiveMethodInfo() =>
			Type == GraphType.PXGraphExtension
				? IsActiveMethodInfo.GetIsActiveMethodInfo(Symbol, _cancellation)
				: null;

		private IsActiveForGraphMethodInfo? GetIsActiveForGraphMethodInfo() =>
			Type == GraphType.PXGraphExtension
				? IsActiveForGraphMethodInfo.GetIsActiveForGraphMethodInfo(Symbol, _cancellation)
				: null;

		private ImmutableArray<ConfigureMethodInfo> GetConfigureMethodOverrides()
		{
			if (Type == GraphType.None)
				return ImmutableArray<ConfigureMethodInfo>.Empty;

			var configureOverrides = ConfigureMethodInfo.GetConfigureMethodInfos(Symbol, Type, PXContext, _cancellation);
			return configureOverrides.ToImmutableArray();
		}

		private ImmutableArray<PXOverrideInfo> GetDeclaredPXOverrideInfos()
		{
			if (Type == GraphType.None)
				return ImmutableArray<PXOverrideInfo>.Empty;

			var pxOverrides = PXOverrideInfo.GetPXOverrides(Symbol, PXContext, _cancellation);
			return pxOverrides.ToImmutableArray();
		}
	}
}