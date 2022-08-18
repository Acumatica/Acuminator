#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class InstanceCreatedEventsAddHandlerWalker : NestedInvocationWalker
	{
		private readonly PXContext _pxContext;
		private int _currentDeclarationOrder;

		public List<InitDelegateInfo> GraphInitDelegates { get; private set; } = new List<InitDelegateInfo>();

		public InstanceCreatedEventsAddHandlerWalker(PXContext pxContext, CancellationToken cancellation)
			: base(pxContext.Compilation, cancellation, pxContext.CodeAnalysisSettings)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_pxContext = pxContext;
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax invocationNode)
		{
			ThrowIfCancellationRequested();

			if (invocationNode.ArgumentList.Arguments.Count == 0)
				return;

			SemanticModel? semanticModel = GetSemanticModel(invocationNode.SyntaxTree);
			IMethodSymbol? methodSymbol = GetSymbol<IMethodSymbol>(invocationNode);

			if (semanticModel == null || methodSymbol == null || methodSymbol.TypeParameters.IsDefaultOrEmpty)
				return;

			bool isCreationDelegateAddition = _pxContext.PXGraph.InstanceCreatedEvents.AddHandler.Equals(methodSymbol.ConstructedFrom);

			if (isCreationDelegateAddition)
			{
				var graphSymbol = methodSymbol.TypeArguments[0] as INamedTypeSymbol;
				var expressionNode = invocationNode.ArgumentList.Arguments.First().Expression;
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

			base.VisitInvocationExpression(invocationNode);
		}
	}
}
