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
		private readonly PXContext _pxContext;
		private int _currentDeclarationOrder;

		public List<InitDelegateInfo> GraphInitDelegates { get; private set; } = new List<InitDelegateInfo>();

		public InstanceCreatedEventsAddHandlerWalker(PXContext pxContext, CancellationToken cancellation)
			: base(pxContext.Compilation, cancellation)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_pxContext = pxContext;
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			SemanticModel semanticModel = _pxContext.Compilation.GetSemanticModel(node.SyntaxTree);

			if (semanticModel.GetSymbolInfo(node, CancellationToken).Symbol is IMethodSymbol symbol)
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
						delegateNode = delegateSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(CancellationToken);
					}

					if (delegateNode != null)
					{
						GraphInitDelegates.Add(new InitDelegateInfo(graphSymbol, delegateSymbol, delegateNode, _currentDeclarationOrder));
						_currentDeclarationOrder++;
					}
				}
			}

			base.VisitInvocationExpression(node);
		}
	}
}
