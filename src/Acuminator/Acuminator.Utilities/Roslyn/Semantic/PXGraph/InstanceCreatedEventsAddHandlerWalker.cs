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
				var graphSymbol = symbol.TypeArguments[0] as INamedTypeSymbol;
				var expressionNode = node.ArgumentList.Arguments.First().Expression;
				var delegateSymbol = semanticModel.GetSymbolInfo(expressionNode, CancellationToken).Symbol;
				var delegateNode = expressionNode is LambdaExpressionSyntax lambdaNode ?
					lambdaNode.Body :
					delegateSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(CancellationToken);

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
