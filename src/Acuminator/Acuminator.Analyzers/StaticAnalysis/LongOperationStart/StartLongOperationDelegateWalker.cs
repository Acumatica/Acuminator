using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
    public class StartLongOperationDelegateWalker : NestedInvocationWalker
    {
        private readonly HashSet<SyntaxNode> _delegates = new HashSet<SyntaxNode>();
        private readonly PXContext _pxContext;

        public ImmutableArray<SyntaxNode> Delegates => _delegates.ToImmutableArray();

        public StartLongOperationDelegateWalker(PXContext pxContext, Compilation compilation, CancellationToken cancellation)
            : base(compilation, cancellation, pxContext.CodeAnalysisSettings)
        {
            pxContext.ThrowOnNull(nameof(pxContext));

            _pxContext = pxContext;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            ThrowIfCancellationRequested();

            IMethodSymbol methodSymbol = GetSymbol<IMethodSymbol>(node);

            if (_pxContext.StartOperation.Contains(methodSymbol))
            {
                var delegateExists = node.ArgumentList?.Arguments.Count > 1;
                if (!delegateExists)
                {
                    return;
                }

                var firstArgument = node.ArgumentList.Arguments[1].Expression;
                if (firstArgument == null)
                {
                    return;
                }

                var delegateBody = GetDelegateBody(firstArgument);
                if (delegateBody == null)
                {
                    return;
                }

                _delegates.Add(delegateBody);
            }
            else
            {
                base.VisitInvocationExpression(node);
            }
        }

        private SyntaxNode GetDelegateBody(ExpressionSyntax expression)
        {
            ThrowIfCancellationRequested();

            if (expression is AnonymousFunctionExpressionSyntax anonymousFunction)
            {
                return anonymousFunction.Body;
            }
            else
            {
                var symbol = GetSymbol<ISymbol>(expression);

                return symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(CancellationToken);
            }
        }
    }
}
