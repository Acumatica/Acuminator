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
        private readonly SemanticModel _semanticModel;
        private readonly CancellationToken _cancellation;
        private readonly PXContext _pxContext;

        public GraphType Type { get; private set; }
        public INamedTypeSymbol Symbol { get; private set; }
        public (ConstructorDeclarationSyntax node, IMethodSymbol symbol) StaticCtrInfo { get; private set; }
        public ImmutableArray<GraphInitializerInfo> Initializers { get; private set; }

        private PXGraphSemanticModel(SemanticModel semanticModel, CancellationToken cancellation, PXContext pxContext, GraphType type, INamedTypeSymbol symbol)
        {
            cancellation.ThrowIfCancellationRequested();

            _semanticModel = semanticModel;
            _cancellation = cancellation;
            _pxContext = pxContext;
            Type = type;
            Symbol = symbol;

            InitStaticCtr();
            InitDeclaredInitializers();
        }

        private void InitStaticCtr()
        {
            _cancellation.ThrowIfCancellationRequested();

            StaticCtrInfo = Symbol.GetDeclaredStaticConstructor(_cancellation);
        }

        private void InitDeclaredInitializers()
        {
            _cancellation.ThrowIfCancellationRequested();

            List<GraphInitializerInfo> initializers = new List<GraphInitializerInfo>();

            if (Type == GraphType.PXGraph)
            {
                IEnumerable<GraphInitializerInfo> ctrs = Symbol.GetDeclaredInstanceConstructors(_cancellation)
                                                         .Select(ctr => new GraphInitializerInfo(GraphInitializerType.InstanceCtr, ctr.node, ctr.symbol));
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
        public static IEnumerable<PXGraphSemanticModel> InferModels(PXContext pxContext, INamedTypeSymbol typeSymbol, SemanticModel semanticModel,
                                                                    CancellationToken cancellation = default)
        {
            cancellation.ThrowIfCancellationRequested();
            pxContext.ThrowOnNull(nameof(pxContext));
            typeSymbol.ThrowOnNull(nameof(typeSymbol));
            semanticModel.ThrowOnNull(nameof(semanticModel));

            List<PXGraphSemanticModel> models = new List<PXGraphSemanticModel>();
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
                PXGraphSemanticModel explicitModel = new PXGraphSemanticModel(semanticModel, cancellation, pxContext, graphType, typeSymbol);

                models.Add(explicitModel);
            }

            IEnumerable<InitDelegateInfo> delegates = GetInitDelegates(semanticModel, cancellation, pxContext, typeSymbol);

            foreach (InitDelegateInfo d in delegates)
            {
                GraphInitializerInfo info = new GraphInitializerInfo(GraphInitializerType.InstanceCreatedDelegate, d.DelegateNode, d.DelegateSymbol);
                PXGraphSemanticModel existingModel = models.FirstOrDefault(m => m.Symbol.Equals(d.GraphTypeSymbol));
                PXGraphSemanticModel implicitModel;

                if (existingModel != null)
                {
                    implicitModel = existingModel;
                }
                else
                {
                    implicitModel = new PXGraphSemanticModel(semanticModel, cancellation, pxContext, d.GraphType, d.GraphTypeSymbol);
                    models.Add(implicitModel);
                }

                implicitModel.Initializers = implicitModel.Initializers.Add(info);
            }

            return models;
        }

        private static IEnumerable<InitDelegateInfo> GetInitDelegates(SemanticModel semanticModel, CancellationToken cancellation, PXContext pxContext,
                                                                         INamedTypeSymbol typeSymbol)
        {
            cancellation.ThrowIfCancellationRequested();

            IEnumerable<SyntaxNode> declaringNodes = typeSymbol.DeclaringSyntaxReferences
                                                     .Select(r => r.GetSyntax(cancellation));
            InstanceCreatedEventsAddHandlerWalker walker = new InstanceCreatedEventsAddHandlerWalker(semanticModel, cancellation, pxContext);

            foreach (SyntaxNode node in declaringNodes)
            {
                cancellation.ThrowIfCancellationRequested();
                walker.Visit(node);
            }

            return walker.GraphInitDelegates;
        }

        private class InstanceCreatedEventsAddHandlerWalker : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly CancellationToken _cancellation;
            private readonly PXContext _pxContext;

            public List<InitDelegateInfo> GraphInitDelegates { get; private set; } = new List<InitDelegateInfo>();

            public InstanceCreatedEventsAddHandlerWalker(SemanticModel semanticModel, CancellationToken cancellation, PXContext pxContext)
            {
                _semanticModel = semanticModel;
                _cancellation = cancellation;
                _pxContext = pxContext;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                _cancellation.ThrowIfCancellationRequested();

                if (_semanticModel.GetSymbolInfo(node, _cancellation).Symbol is IMethodSymbol symbol)
                {
                    bool isCreationDelegateAddition = _pxContext.PXGraphRelatedMethods.InstanceCreatedEventsAddHandler.Equals(symbol.ConstructedFrom);

                    if (isCreationDelegateAddition)
                    {
                        INamedTypeSymbol graphSymbol = symbol.TypeArguments[0] as INamedTypeSymbol;
                        ExpressionSyntax delegateNode = node.ArgumentList.Arguments.First().Expression;
                        ISymbol delegateSymbol = _semanticModel.GetSymbolInfo(delegateNode).Symbol;

                        GraphInitDelegates.Add(new InitDelegateInfo(graphSymbol, delegateSymbol, delegateNode));
                    }
                }

                base.VisitInvocationExpression(node);
            }
        }

        private class InitDelegateInfo
        {
            public GraphType GraphType => GraphType.PXGraph;
            public INamedTypeSymbol GraphTypeSymbol { get; private set; }
            public ISymbol DelegateSymbol { get; private set; }
            public ExpressionSyntax DelegateNode { get; private set; }

            public InitDelegateInfo(INamedTypeSymbol graphSymbol, ISymbol delegateSymbol, ExpressionSyntax delegateNode)
            {
                GraphTypeSymbol = graphSymbol;
                DelegateSymbol = delegateSymbol;
                DelegateNode = delegateNode;
            }
        }
    }
}
