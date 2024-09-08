#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class PXGraphSemanticModel : ISemanticModel
	{
		protected readonly CancellationToken _cancellation;

		public PXContext PXContext { get; }

		public GraphSemanticModelCreationOptions ModelCreationOptions { get; }

		public bool IsProcessing { get; private set; }

		public GraphType GraphType { get; }

		public GraphOrGraphExtInfoBase GraphOrGraphExtInfo { get; }

		[MemberNotNullWhen(returnValue: false, nameof(Node))]
		public bool IsInMetadata => GraphOrGraphExtInfo.IsInMetadata;

		[MemberNotNullWhen(returnValue: true, nameof(Node))]
		public bool IsInSource => GraphOrGraphExtInfo.IsInSource;

		public INamedTypeSymbol Symbol => GraphOrGraphExtInfo.Symbol;

		public ClassDeclarationSyntax? Node => GraphOrGraphExtInfo.Node;

		public int DeclarationOrder => GraphOrGraphExtInfo.DeclarationOrder;

		/// <summary>
		/// The graph symbol. For a graph, the value is the same as <see cref="Symbol"/>. 
		/// For a graph extension, the value is the symbol of the extension's base graph.
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
		/// Actions which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<ActionInfo> DeclaredActions => 
			Actions.Where(action => action.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Action handlers which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<ActionHandlerInfo> DeclaredActionHandlers =>
			ActionHandlers.Where(handler => handler.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Views which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<DataViewInfo> DeclaredViews =>
			Views.Where(view => view.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// View delegates which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<DataViewDelegateInfo> DeclaredViewDelegates =>
			ViewDelegates.Where(viewDelegate => viewDelegate.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Information about the IsActive method of the graph extensions. 
		/// The value can be <c>null</c>. The value is always <c>null</c> for a graph.
		/// </summary>
		/// <value>
		/// Information about the IsActive method.
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
		/// Information about the graph's or the graph extension's Configure method override. The override can be declared in base types.
		/// </summary>
		public ConfigureMethodInfo? ConfigureMethodOverride { get; }

		/// <summary>
		/// Information about the Configure method override declared in this type. <see langword="null"/> if the method is not declared in this type.
		/// </summary>
		public ConfigureMethodInfo? DeclaredConfigureMethodOverride =>
			ConfigureMethodOverride != null && ConfigureMethodOverride.Symbol.IsDeclaredInType(Symbol)
				? ConfigureMethodOverride
				: null;

		/// <summary>
		/// An indicator of whether the graph or the graph extension configures a workflow.
		/// </summary>
		[MemberNotNullWhen(returnValue: true, nameof(ConfigureMethodOverride))]
		public bool ConfiguresWorkflow => ConfigureMethodOverride != null;

		/// <summary>
		/// Information about the graph's or the graph extension's Initialize method and its overrides. The method can be declared in base types.
		/// </summary>
		public InitializeMethodInfo? InitializeMethodInfo { get; }

		/// <summary>
		/// Information about the Initialize method declared in this type. <see langword="null"/> if the method is not declared in this type.
		/// </summary>
		public InitializeMethodInfo? DeclaredInitializeMethodInfo =>
			InitializeMethodInfo != null && InitializeMethodInfo.Symbol.IsDeclaredInType(Symbol)
				? InitializeMethodInfo
				: null;

		/// <summary>
		/// An indicator of whether the graph extension has the PXProtectedAccess attribute.
		/// </summary>
		public bool HasPXProtectedAccess { get; }

		/// <summary>
		/// The attributes declared on the graph or the graph extension.
		/// </summary>
		public ImmutableArray<GraphAttributeInfo> Attributes { get; }

		protected PXGraphSemanticModel(PXContext pxContext, GraphType graphType, INamedTypeSymbol symbol, ClassDeclarationSyntax? node,
										GraphSemanticModelCreationOptions modelCreationOptions, int declarationOrder,
										CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();

			PXContext = pxContext.CheckIfNull();
			GraphType = graphType;

			if (GraphType == GraphType.PXGraph)
			{
				GraphOrGraphExtInfo = new GraphInfo(node, symbol, declarationOrder);
				GraphSymbol = Symbol;
			}
			else
			{
				GraphOrGraphExtInfo = new GraphExtensionInfo(node, symbol, declarationOrder);
				GraphSymbol = Symbol.GetGraphFromGraphExtension(PXContext);
			}

			_cancellation 		 = cancellation;
			ModelCreationOptions = modelCreationOptions;
			Attributes			 = GetGraphAttributes();

			StaticConstructors 	 = Symbol.GetStaticConstructors(_cancellation);
			ViewsByNames 		 = GetDataViews();
			ViewDelegatesByNames = GetDataViewDelegates();

			ActionsByNames 		  = GetActions();
			ActionHandlersByNames = GetActionHandlers();

			InitProcessingDelegatesInfo();

			Initializers 			   = GetDeclaredInitializers().ToImmutableArray();
			IsActiveMethodInfo 		   = GetIsActiveMethodInfo();
			IsActiveForGraphMethodInfo = GetIsActiveForGraphMethodInfo();
			ConfigureMethodOverride	   = ConfigureMethodInfo.GetConfigureMethodInfo(Symbol, GraphType, PXContext, _cancellation);
			InitializeMethodInfo	   = InitializeMethodInfo.GetInitializeMethodInfo(Symbol, GraphType, PXContext, _cancellation);

			PXOverrides = GetDeclaredPXOverrideInfos();
			HasPXProtectedAccess = IsPXProtectedAccessAttributeDeclared();
		}

		protected void InitProcessingDelegatesInfo()
		{
			if (!ModelCreationOptions.HasFlag(GraphSemanticModelCreationOptions.CollectProcessingDelegates))
			{
				IsProcessing = Views.Any(v => v.IsProcessing);
				return;
			}

			var processingViewSymbols = Views.Where(v => v.IsProcessing)
											 .Select(v => v.Symbol)
											 .ToImmutableHashSet(SymbolEqualityComparer.Default);
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

		protected ImmutableArray<GraphAttributeInfo> GetGraphAttributes()
		{
			var attributes = Symbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return ImmutableArray<GraphAttributeInfo>.Empty;

			var attributeInfos = attributes.Select((attributeData, relativeOrder) => new GraphAttributeInfo(PXContext, attributeData, relativeOrder));
			var builder = ImmutableArray.CreateBuilder<GraphAttributeInfo>(attributes.Length);
			builder.AddRange(attributeInfos);

			return builder.ToImmutable();
		}

		protected ImmutableDictionary<string, DataViewInfo> GetDataViews() =>
			GetInfos(() => Symbol.GetViewsWithSymbolsFromPXGraph(PXContext),
					 () => Symbol.GetViewsFromGraphExtensionAndBaseGraph(PXContext));

		protected ImmutableDictionary<string, DataViewDelegateInfo> GetDataViewDelegates() =>
			GetInfos(() => Symbol.GetViewDelegatesFromGraph(ViewsByNames, PXContext, cancellation: _cancellation),
					 () => Symbol.GetViewDelegatesFromGraphExtensionAndBaseGraph(ViewsByNames, PXContext, _cancellation));

		protected ImmutableDictionary<string, ActionInfo> GetActions() =>
			GetInfos(() => Symbol.GetActionSymbolsWithTypesFromGraph(PXContext),
					 () => Symbol.GetActionsFromGraphExtensionAndBaseGraph(PXContext));

		protected ImmutableDictionary<string, ActionHandlerInfo> GetActionHandlers() =>
			GetInfos(() => Symbol.GetActionHandlersFromGraph(ActionsByNames, PXContext, cancellation: _cancellation),
					 () => Symbol.GetActionHandlersFromGraphExtensionAndBaseGraph(ActionsByNames, PXContext, _cancellation));

		protected ImmutableDictionary<string, TInfo> GetInfos<TInfo>(Func<OverridableItemsCollection<TInfo>> graphInfosSelector,
																   Func<OverridableItemsCollection<TInfo>> graphExtInfosSelector)
		where TInfo : IOverridableItem<TInfo>
		{
			var infos = GraphType == GraphType.PXGraph
				? graphInfosSelector()
				: graphExtInfosSelector();

			return infos.ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);
		}

		protected IEnumerable<GraphInitializerInfo> GetDeclaredInitializers()
		{
			_cancellation.ThrowIfCancellationRequested();

			if (GraphType == GraphType.PXGraph)
			{
				return Symbol.GetDeclaredInstanceConstructors(_cancellation)
							 .Select((ctr, order) => new GraphInitializerInfo(GraphInitializerType.InstanceConstructor, ctr.Node, ctr.Symbol, order));
			}
			else
			{
				(MethodDeclarationSyntax node, IMethodSymbol symbol) = Symbol.GetGraphExtensionInitialization(PXContext, _cancellation);

				if (node != null && symbol != null)
				{
					return new GraphInitializerInfo(GraphInitializerType.InitializeMethod, node, symbol, declarationOrder: 0)
								.ToEnumerable();
				}

				return [];
			}
		}

		/// <summary>
		/// Returns one or multiple semantic models of the PXGraph and PXGraphExtension descendants which are inferred from <paramref name="typeSymbol"/>.
		/// </summary>
		/// <param name="pxContext">Context instance</param>
		/// <param name="typeSymbol">Symbol which is PXGraph or PXGraphExtension descendant and/or which uses the PXGraph.InstanceCreated AddHandler method</param>
		/// <param name="semanticModel">Semantic model</param>
		/// <param name="cancellation">Cancellation token</param>
		/// <returns></returns>
		public static IEnumerable<PXGraphSemanticModel> InferModels(PXContext pxContext, INamedTypeSymbol typeSymbol,
																	GraphSemanticModelCreationOptions modelCreationOptions,
																	int? declarationOrder = null, CancellationToken cancellation = default)
		{
			pxContext.ThrowOnNull();
			typeSymbol.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			var models = new List<PXGraphSemanticModel>(capacity: 1);
			var explicitModel = InferExplicitModel(pxContext, typeSymbol, modelCreationOptions, declarationOrder, cancellation);

			if (explicitModel != null)
			{
				models.Add(explicitModel);

				if (declarationOrder.HasValue)
					declarationOrder = declarationOrder.Value + 1;
			}

			if (modelCreationOptions.HasFlag(GraphSemanticModelCreationOptions.InferImplicitModels))
			{
				InferImplicitModels(pxContext, typeSymbol, modelCreationOptions, models, declarationOrder, cancellation);
			}

			return models;
		}

		private static void InferImplicitModels(PXContext pxContext, INamedTypeSymbol typeSymbol,
												GraphSemanticModelCreationOptions modelCreationOptions,
												List<PXGraphSemanticModel> models, int? startDeclarationOrder, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			IEnumerable<InitDelegateInfo> delegates = GetInitDelegates(pxContext, typeSymbol, startDeclarationOrder, cancellation);

			foreach (InitDelegateInfo initDelegateInfo in delegates)
			{
				GraphInitializerInfo info = new GraphInitializerInfo(GraphInitializerType.InstanceCreatedDelegate, initDelegateInfo.Node,
																	 initDelegateInfo.Symbol, initDelegateInfo.DeclarationOrder);
				PXGraphSemanticModel existingModel = models.FirstOrDefault(m => m.Symbol.Equals(initDelegateInfo.GraphTypeSymbol, 
																								SymbolEqualityComparer.Default));
				PXGraphSemanticModel implicitModel;

				if (existingModel != null)
				{
					implicitModel = existingModel;
				}
				else
				{
					var graphOrExtNode = initDelegateInfo.GraphTypeSymbol.GetSyntax(cancellation) as ClassDeclarationSyntax;
					implicitModel = new PXGraphSemanticModel(pxContext, initDelegateInfo.GraphType, initDelegateInfo.GraphTypeSymbol,
															 graphOrExtNode, modelCreationOptions, initDelegateInfo.DeclarationOrder, cancellation);
					models.Add(implicitModel);
				}

				implicitModel.Initializers = implicitModel.Initializers.Add(info);
			}
		}

		public static PXGraphSemanticModel? InferExplicitModel(PXContext pxContext, INamedTypeSymbol graphOrGraphExtTypeSymbol,
															  GraphSemanticModelCreationOptions modelCreationOptions,
															  int? declarationOrder = null, CancellationToken cancellation = default)
		{
			pxContext.ThrowOnNull();
			graphOrGraphExtTypeSymbol.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			GraphType graphType;

			if (graphOrGraphExtTypeSymbol.IsPXGraph(pxContext))
			{
				graphType = GraphType.PXGraph;
			}
			else if (graphOrGraphExtTypeSymbol.IsPXGraphExtension(pxContext))
			{
				graphType = GraphType.PXGraphExtension;
			}
			else
				return null;

			var graphOrExtNode = graphOrGraphExtTypeSymbol.GetSyntax(cancellation) as ClassDeclarationSyntax;
			return new PXGraphSemanticModel(pxContext, graphType, graphOrGraphExtTypeSymbol, graphOrExtNode, modelCreationOptions, 
											declarationOrder ?? 0, cancellation);
		}

		protected static IEnumerable<InitDelegateInfo> GetInitDelegates(PXContext pxContext, INamedTypeSymbol typeSymbol,
																		int? startDeclarationOrder, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			IEnumerable<SyntaxNode> declaringNodes = typeSymbol.DeclaringSyntaxReferences
															   .Select(r => r.GetSyntax(cancellation));
			InstanceCreatedEventsAddHandlerWalker walker = new InstanceCreatedEventsAddHandlerWalker(pxContext, startDeclarationOrder, cancellation);

			foreach (SyntaxNode node in declaringNodes)
			{
				cancellation.ThrowIfCancellationRequested();
				walker.Visit(node);
			}

			return walker.GraphInitDelegates;
		}

		protected IsActiveMethodInfo? GetIsActiveMethodInfo() =>
			GraphType == GraphType.PXGraphExtension
				? IsActiveMethodInfo.GetIsActiveMethodInfo(Symbol, _cancellation)
				: null;

		protected IsActiveForGraphMethodInfo? GetIsActiveForGraphMethodInfo() =>
			GraphType == GraphType.PXGraphExtension
				? IsActiveForGraphMethodInfo.GetIsActiveForGraphMethodInfo(Symbol, _cancellation)
				: null;

		protected ImmutableArray<PXOverrideInfo> GetDeclaredPXOverrideInfos()
		{
			var pxOverrides = PXOverrideInfo.GetPXOverrides(Symbol, PXContext, _cancellation);
			return pxOverrides.ToImmutableArray();
		}

		protected bool IsPXProtectedAccessAttributeDeclared() =>
			GraphType == GraphType.PXGraphExtension && !Attributes.IsDefaultOrEmpty && 
			Attributes.Any(attrInfo => attrInfo.IsProtectedAccess);
	}
}