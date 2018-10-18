using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        public ImmutableArray<StaticConstructorInfo> StaticConstructors { get; }
        public ImmutableArray<GraphInitializerInfo> Initializers { get; private set; }

        public ImmutableDictionary<string, DataViewInfo> ViewsByNames { get; }
        public IEnumerable<DataViewInfo> Views => ViewsByNames.Values;

        public ImmutableDictionary<string, DataViewDelegateInfo> ViewDelegatesByNames { get; }
        public IEnumerable<DataViewDelegateInfo> ViewDelegates => ViewDelegatesByNames.Values;

        private PXGraphSemanticModel(Compilation compilation, PXContext pxContext, GraphType type,
                                     INamedTypeSymbol symbol, CancellationToken cancellation = default)
        {
            cancellation.ThrowIfCancellationRequested();

            _pxContext = pxContext;
            Type = type;
            Symbol = symbol;
            _cancellation = cancellation;
            StaticConstructors = Symbol.GetStaticConstructors(_cancellation);
            ViewsByNames = GetDataViews();
            ViewDelegatesByNames = GetDataViewDelegates();
            IsProcessing = Views.Any(v => v.IsProcessing);

            InitProcessingDelegatesInfo(compilation);
            InitDeclaredInitializers();
        }

        private void InitProcessingDelegatesInfo(Compilation compilation)
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
            var walker = new ProcessingDelegatesWalker(compilation, _pxContext, processingViewSymbols, _cancellation);

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
            if (Type == GraphType.PXGraph)
            {
                var graphViews = Symbol.GetViewsWithSymbolsFromPXGraph(_pxContext);

                return graphViews
                       .ToImmutableDictionary(v => v.ViewSymbol.Name,
                                              v => new DataViewInfo(v.ViewSymbol, v.ViewType, _pxContext),
                                              StringComparer.OrdinalIgnoreCase);
            }

            var extViews = Symbol.GetViewsFromGraphExtensionAndBaseGraph(_pxContext);

            return extViews
                   .ToImmutableDictionary(v => v.Item.ViewSymbol.Name,
                                          v => v.Base == null
                                               ? new DataViewInfo(v.Item.ViewSymbol,
                                                                  v.Item.ViewType,
                                                                  _pxContext)
                                               : new DataViewInfo(v.Item.ViewSymbol,
                                                                  v.Item.ViewType,
                                                                  _pxContext,
                                                                  new DataViewInfo(v.Base.Item.ViewSymbol, v.Base.Item.ViewType, _pxContext)));
        }

        private ImmutableDictionary<string, DataViewDelegateInfo> GetDataViewDelegates()
        {
            var viewSymbols = Views.Select(v => v.Symbol);

            if (Type == GraphType.PXGraph)
            {
                var graphDelegates = Symbol.GetViewDelegatesFromGraph(viewSymbols, _pxContext, _cancellation);

                return graphDelegates
                       .ToImmutableDictionary(d => d.Symbol.Name,
                                              d => new DataViewDelegateInfo(d.Node, d.Symbol));
            }

            var extDelegates = Symbol.GetViewDelegatesFromGraphExtensionAndBaseGraph(viewSymbols, _pxContext, _cancellation);

            return extDelegates
                   .ToImmutableDictionary(d => d.Item.Symbol.Name,
                                          d => d.Base == null
                                               ? new DataViewDelegateInfo(d.Item.Node, d.Item.Symbol)
                                               : new DataViewDelegateInfo(d.Item.Node, d.Item.Symbol, new DataViewDelegateInfo(d.Base.Item.Node, d.Base.Item.Symbol)));
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
        public static IEnumerable<PXGraphSemanticModel> InferModels(Compilation compilation, PXContext pxContext,
                                                                    INamedTypeSymbol typeSymbol, CancellationToken cancellation = default)
        {
            cancellation.ThrowIfCancellationRequested();
            compilation.ThrowOnNull(nameof(compilation));
            pxContext.ThrowOnNull(nameof(pxContext));
            typeSymbol.ThrowOnNull(nameof(typeSymbol));

            List<PXGraphSemanticModel> models = new List<PXGraphSemanticModel>();

            InferExplicitModel(compilation, pxContext, typeSymbol, models, cancellation);
            InferImplicitModels(compilation, pxContext, typeSymbol, models, cancellation);

            return models;
        }

        private static void InferImplicitModels(Compilation compilation, PXContext pxContext, INamedTypeSymbol typeSymbol,
                                                List<PXGraphSemanticModel> models, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            IEnumerable<InitDelegateInfo> delegates = GetInitDelegates(compilation, pxContext, typeSymbol, cancellation);

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
                    implicitModel = new PXGraphSemanticModel(compilation, pxContext, d.GraphType, d.GraphTypeSymbol, cancellation);
                    models.Add(implicitModel);
                }

                implicitModel.Initializers = implicitModel.Initializers.Add(info);
            }
        }

        private static void InferExplicitModel(Compilation compilation, PXContext pxContext, INamedTypeSymbol typeSymbol,
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
                PXGraphSemanticModel explicitModel = new PXGraphSemanticModel(compilation, pxContext, graphType, typeSymbol, cancellation);

                models.Add(explicitModel);
            }
        }

        private static IEnumerable<InitDelegateInfo> GetInitDelegates(Compilation compilation, PXContext pxContext,
                                                                      INamedTypeSymbol typeSymbol, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            IEnumerable<SyntaxNode> declaringNodes = typeSymbol.DeclaringSyntaxReferences
                                                     .Select(r => r.GetSyntax(cancellation));
            InstanceCreatedEventsAddHandlerWalker walker = new InstanceCreatedEventsAddHandlerWalker(compilation, pxContext, cancellation);

            foreach (SyntaxNode node in declaringNodes)
            {
                cancellation.ThrowIfCancellationRequested();
                walker.Visit(node);
            }

            return walker.GraphInitDelegates;
        }
    }
}
