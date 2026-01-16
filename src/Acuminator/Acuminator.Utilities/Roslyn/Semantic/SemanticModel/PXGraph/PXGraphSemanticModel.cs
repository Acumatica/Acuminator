using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Shared;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Graph;
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

		public string Name => GraphOrGraphExtInfo.Name;

		[MemberNotNullWhen(returnValue: false, nameof(Node))]
		public bool IsInMetadata => GraphOrGraphExtInfo.IsInMetadata;

		[MemberNotNullWhen(returnValue: true, nameof(Node))]
		public bool IsInSource => GraphOrGraphExtInfo.IsInSource;

		public ITypeSymbol Symbol => GraphOrGraphExtInfo.Symbol;

		public ClassDeclarationSyntax? Node => GraphOrGraphExtInfo.Node;

		public int DeclarationOrder => GraphOrGraphExtInfo.DeclarationOrder;

		/// <summary>
		/// The graph symbol. For a graph, the value is the same as <see cref="Symbol"/>. 
		/// For a graph extension, the value is the symbol of the extension's base graph.
		/// </summary>
		public ITypeSymbol? GraphSymbol { get; }

		public ImmutableArray<StaticConstructorInfo> StaticConstructors { get; }

		/// <summary>
		/// Gets or sets the initializers.
		/// </summary>
		/// <remarks>
		/// By initializers Acuminator understands special code elements of graph or graph extension that configure graph's initial state.<br/>
		/// Currently, initializers consists of:
		/// <list type="bullet">
		/// <item>Graph and graph extension constructors.</item>
		/// <item><c>Initialize</c> method override of a graph extension.</item>
		/// <item><c>Initialize</c> method of a graph that implements <c>PX.Data.DependencyInjection.IGraphWithInitialization</c> interface.</item>
		/// <item><c>Configure</c> method override of a graph or graph extension that configure screen workflow.</item>
		/// </list>
		/// </remarks>
		/// <value>
		/// The initializers.
		/// </value>
		public ImmutableArray<GraphInitializerInfo> DeclaredInitializers { get; private set; }

		public ImmutableDictionary<string, DataViewInfo> ViewsByNames { get; }
		public IEnumerable<DataViewInfo> Views => ViewsByNames.Values;

		public ImmutableDictionary<string, DataViewDelegateInfo> ViewDelegatesByNames { get; }
		public IEnumerable<DataViewDelegateInfo> ViewDelegates => ViewDelegatesByNames.Values;

		public ImmutableDictionary<string, ActionInfo> ActionsByNames { get; }
		public IEnumerable<ActionInfo> Actions => ActionsByNames.Values;

		public ImmutableDictionary<string, ActionDelegateInfo> ActionDelegateByNames { get; }
		public IEnumerable<ActionDelegateInfo> ActionDelegates => ActionDelegateByNames.Values;

		public ImmutableArray<PXOverrideInfo> DeclaredPXOverrides { get; }

		/// <summary>
		/// Actions which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<ActionInfo> DeclaredActions => 
			Actions.Where(action => action.Symbol.IsDeclaredInType(Symbol));

		/// <summary>
		/// Action delegates which are declared in the graph or the graph extension that is represented by this instance of the semantic model.
		/// </summary>
		public IEnumerable<ActionDelegateInfo> DeclaredActionDelegates =>
			ActionDelegates.Where(handler => handler.Symbol.IsDeclaredInType(Symbol));

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

		protected PXGraphSemanticModel(PXContext pxContext, GraphOrGraphExtInfoBase graphOrGraphExtInfo, GraphSemanticModelCreationOptions modelCreationOptions,
									   CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			PXContext 			= pxContext.CheckIfNull();
			GraphOrGraphExtInfo = graphOrGraphExtInfo.CheckIfNull();

			(GraphType, GraphSymbol) = graphOrGraphExtInfo switch
			{
				GraphInfo graphInfo 			=> (GraphType.PXGraph, graphInfo.Symbol),
				GraphExtensionInfo graphExtInfo => (GraphType.PXGraphExtension, graphExtInfo.BaseGraph?.Symbol),
				_ 								=> throw new ArgumentOutOfRangeException(nameof(graphOrGraphExtInfo),
														$"The \"{nameof(graphOrGraphExtInfo)}\" parameter must be either {nameof(GraphInfo)} or {nameof(GraphExtensionInfo)}.")
			};

			_cancellation 		 = cancellation;
			ModelCreationOptions = modelCreationOptions;
			Attributes			 = GetGraphAttributes();

			StaticConstructors 	 = GraphOrGraphExtInfo.Symbol.GetStaticConstructors(_cancellation);
			ViewsByNames 		 = GetDataViews();
			ViewDelegatesByNames = GetDataViewDelegates(ViewsByNames);

			ActionsByNames 		  = GetActions();
			ActionDelegateByNames = GetActionDelegates(ActionsByNames);

			InitProcessingDelegatesInfo();

			ConfigureMethodOverride = ConfigureMethodInfo.GetConfigureMethodInfo(GraphOrGraphExtInfo.Symbol, GraphType, PXContext, _cancellation);
			InitializeMethodInfo	= InitializeMethodInfo.GetInitializeMethodInfo(GraphOrGraphExtInfo.Symbol, GraphType, PXContext, _cancellation);

			DeclaredInitializers 	   = GetDeclaredInitializers().ToImmutableArray();
			IsActiveMethodInfo 		   = GetIsActiveMethodInfo();
			IsActiveForGraphMethodInfo = GetIsActiveForGraphMethodInfo();
			
			DeclaredPXOverrides = GetDeclaredPXOverrideInfos();
			HasPXProtectedAccess = IsPXProtectedAccessAttributeDeclared();
		}

		protected void InitProcessingDelegatesInfo()
		{
			if (ViewsByNames.Count == 0)
			{
				IsProcessing = false;
				return;
			}

			if (!ModelCreationOptions.HasFlag(GraphSemanticModelCreationOptions.CollectProcessingDelegates))
			{
				IsProcessing = Views.Any(v => v.IsProcessing);
				return;
			}

			var processingViewSymbols = Views.Where(v => v.IsProcessing)
											 .Select(v => v.Symbol)
											 .ToHashSet(SymbolEqualityComparer.Default);
			IsProcessing = processingViewSymbols.Count > 0;

			if (!IsProcessing)
				return;

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
			GraphOrGraphExtInfo.GetViewInfos(PXContext, _cancellation)
							   .ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);

		protected ImmutableDictionary<string, DataViewDelegateInfo> GetDataViewDelegates(IDictionary<string, DataViewInfo> viewsByName) =>
			GraphOrGraphExtInfo.GetViewDelegateInfos(PXContext, viewsByName, _cancellation)
							   .ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);

		protected ImmutableDictionary<string, ActionInfo> GetActions() =>
			GraphOrGraphExtInfo.GetActionInfos(PXContext, _cancellation)
							   .ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);

		protected ImmutableDictionary<string, ActionDelegateInfo> GetActionDelegates(IDictionary<string, ActionInfo> actionsByName) =>
			GraphOrGraphExtInfo.GetActionDelegateInfos(PXContext, actionsByName, _cancellation)
							   .ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets the declared initializers in this collection.
		/// </summary>
		/// <remarks>
		/// By initializer Acuminator understands special code elements of graph or graph extension that configure graph's initial state.<br/>
		/// Currently, initializers consists of:
		/// <list type="bullet">
		/// <item>Graph and graph extension constructors.</item>
		/// <item><c>Initialize</c> method override of a graph extension.</item>
		/// <item><c>Initialize</c> method of a graph that implements <c>PX.Data.DependencyInjection.IGraphWithInitialization</c> interface.</item>
		/// <item><c>Configure</c> method override of a graph or graph extension that configure screen workflow.</item>
		/// </list>
		/// </remarks>
		/// <returns>
		/// The declared initializers in this collection.
		/// </returns>
		protected List<GraphInitializerInfo> GetDeclaredInitializers()
		{
			_cancellation.ThrowIfCancellationRequested();

			var instanceConstructors = Symbol.GetDeclaredInstanceConstructors(_cancellation);
			List<GraphInitializerInfo> initializerInfos;

			if (instanceConstructors.Count > 0)
			{
				var constructorInitializerInfos = 
					instanceConstructors.Select((constructor, order) => new GraphInitializerInfo(GraphInitializerType.InstanceConstructor,
																								constructor.Node, constructor.Symbol, order));
				initializerInfos = constructorInitializerInfos.ToList(capacity: 4);
			}
			else
			{
				initializerInfos = [];
			}

			int declarationOrder = initializerInfos.Count;

			if (DeclaredInitializeMethodInfo is { } declaredInitializeMethod)
			{
				var initializeMethodInfo = new GraphInitializerInfo(GraphInitializerType.InitializeMethod, declaredInitializeMethod.Node,
																	declaredInitializeMethod.Symbol, declarationOrder);
				declarationOrder++;
				initializerInfos.Add(initializeMethodInfo);
			}

			if (DeclaredConfigureMethodOverride is { } declaredConfigureMethod)
			{
				var configureMethodInfo = new GraphInitializerInfo(GraphInitializerType.ConfigureMethod, declaredConfigureMethod.Node,
																	declaredConfigureMethod.Symbol, declarationOrder);
				initializerInfos.Add(configureMethodInfo);
			}

			return initializerInfos;
		}

		/// <summary>
		/// Returns the semantic model of graph or graph extension which is inferred from <paramref name="graphOrGraphExtensionTypeSymbol"/>.
		/// </summary>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="graphOrGraphExtensionTypeSymbol">The graph or graph extension type symbol.</param>
		/// <param name="modelCreationOptions">Options for controlling the semantic model creation.</param>
		/// <param name="customDeclarationOrder">(Optional) The declaration order.</param>
		/// <param name="cancellation">(Optional) Cancellation token.</param>
		/// <returns>
		/// A semantic model for a given graph or graph extension type <paramref name="graphOrGraphExtensionTypeSymbol"/>.
		/// </returns>
		public static PXGraphSemanticModel? InferModel(PXContext pxContext, ITypeSymbol? graphOrGraphExtensionTypeSymbol,
													   GraphSemanticModelCreationOptions modelCreationOptions,
													   int? customDeclarationOrder = null, CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();

			var inferredInfo = GraphAndGraphExtInfoBuilder.Instance.InferTypeInfo(graphOrGraphExtensionTypeSymbol, pxContext, 
																				  customDeclarationOrder, cancellation);

			if (inferredInfo?.GetResultKind() != InferResultKind.Success || inferredInfo.InferredInfo is not GraphOrGraphExtInfoBase graphOrGraphExt)
				return null;

			return InferModel(pxContext, graphOrGraphExt, modelCreationOptions, cancellation);
		}

		/// <summary>
		/// Infer semantic model for a given <paramref name="graphOrGraphExtInferredInfo"/>.
		/// If <paramref name="graphOrGraphExtInferredInfo"/> is not a graph or graph extension, returns <see langword="null"/>.
		/// </summary>
		/// <param name="pxContext">Acumatica context.</param>
		/// <param name="graphOrGraphExtInferredInfo">
		/// The graph or graph extension inferred information obtained from resolving a hierarchy of chained graph extensions and base types.
		/// </param>
		/// <param name="modelCreationOptions">Options for controlling the semantic model creation.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// A semantic model for a given graph or graph extension <paramref name="graphOrGraphExtInferredInfo"/>.<br/>
		/// If <paramref name="graphOrGraphExtInferredInfo"/> is not graph or graph extension, then returns <see langword="null"/>.
		/// </returns>
		public static PXGraphSemanticModel? InferModel(PXContext pxContext, GraphOrGraphExtInfoBase graphOrGraphExtInferredInfo,
													   GraphSemanticModelCreationOptions modelCreationOptions,
													   CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (graphOrGraphExtInferredInfo is not (GraphInfo or GraphExtensionInfo))
				return null;

			return new PXGraphSemanticModel(pxContext, graphOrGraphExtInferredInfo, modelCreationOptions, cancellation);
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
			if (GraphOrGraphExtInfo is GraphExtensionInfo graphExtensionInfo)
			{
				var pxOverrides = PXOverrideInfo.GetDeclaredPXOverrides(graphExtensionInfo, PXContext, _cancellation);
				return pxOverrides.ToImmutableArray();
			}
			else
				return ImmutableArray<PXOverrideInfo>.Empty;
		}

		protected bool IsPXProtectedAccessAttributeDeclared() =>
			GraphType == GraphType.PXGraphExtension && !Attributes.IsDefaultOrEmpty && 
			Attributes.Any(attrInfo => attrInfo.IsProtectedAccess);
	}
}