using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class InstanceCreatedEventsAddHandlerWalker : NestedInvocationWalker
    {
        private readonly CancellationToken _cancellation;
        private readonly PXContext _pxContext;

        public List<InitDelegateInfo> GraphInitDelegates { get; private set; } = new List<InitDelegateInfo>();

        public InstanceCreatedEventsAddHandlerWalker(Compilation compilation, PXContext pxContext, CancellationToken cancellation)
            : base(compilation, cancellation)
        {
            pxContext.ThrowOnNull(nameof(pxContext));

            _pxContext = pxContext;
            _cancellation = cancellation;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            _cancellation.ThrowIfCancellationRequested();

            SemanticModel semanticModel = _pxContext.Compilation.GetSemanticModel(node.SyntaxTree);

            if (semanticModel.GetSymbolInfo(node, _cancellation).Symbol is IMethodSymbol symbol)
            {
                bool isCreationDelegateAddition = _pxContext.PXGraph.InstanceCreatedEvents.AddHandler.Equals(symbol.ConstructedFrom);

                if (isCreationDelegateAddition)
                {
                    INamedTypeSymbol graphSymbol = symbol.TypeArguments[0] as INamedTypeSymbol;
                    SyntaxNode expressionNode = node.ArgumentList.Arguments.First().Expression;
                    SyntaxNode delegateNode;
                    ISymbol delegateSymbol;

                    if (expressionNode is LambdaExpressionSyntax lambdaNode)
                    {
                        delegateNode = lambdaNode.Body;
                        delegateSymbol = semanticModel.GetSymbolInfo(delegateNode).Symbol;
                    }
                    else
                    {
                        delegateSymbol = semanticModel.GetSymbolInfo(expressionNode).Symbol;
                        delegateNode = delegateSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(_cancellation);
                    }

                    if (delegateNode != null)
                    {
                        GraphInitDelegates.Add(new InitDelegateInfo(graphSymbol, delegateSymbol, delegateNode));
                    }
                }
            }

            base.VisitInvocationExpression(node);
        }
    }
}
