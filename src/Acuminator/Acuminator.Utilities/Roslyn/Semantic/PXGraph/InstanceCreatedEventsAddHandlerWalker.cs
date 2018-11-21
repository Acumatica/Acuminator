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

			SemanticModel semanticModel = GetSemanticModel(node.SyntaxTree);
			IMethodSymbol symbol = GetSymbol<IMethodSymbol>(node);

			if (semanticModel == null || symbol == null)
				return;

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
					delegateSymbol = semanticModel.GetSymbolInfo(delegateNode, CancellationToken).Symbol;
				}
				else
				{
					delegateSymbol = semanticModel.GetSymbolInfo(expressionNode, CancellationToken).Symbol;
					delegateNode = delegateSymbol?.DeclaringSyntaxReferences
												  .FirstOrDefault()?
												  .GetSyntax(CancellationToken);
				}

				if (delegateNode != null)
				{
					GraphInitDelegates.Add(new InitDelegateInfo(graphSymbol, delegateSymbol, delegateNode, _currentDeclarationOrder));
					_currentDeclarationOrder++;
				}
			}

			base.VisitInvocationExpression(node);
		}
	}
}
