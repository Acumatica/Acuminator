#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class InstanceCreatedEventsAddHandlerWalker : NestedInvocationWalker
	{
		private readonly PXContext _pxContext;
		private int _currentDeclarationOrder = 0;

		public List<InitDelegateInfo> GraphInitDelegates { get; } = new List<InitDelegateInfo>();

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
				AddGraphInitDelegate(invocationNode, methodSymbol, semanticModel);
			}

			base.VisitInvocationExpression(invocationNode);
		}

		private void AddGraphInitDelegate(InvocationExpressionSyntax invocationNode, IMethodSymbol methodSymbol, SemanticModel semanticModel)
		{
			ITypeSymbol graphSymbol = methodSymbol.TypeArguments[0];

			if (graphSymbol == null)
				return;

			var expressionNode = invocationNode.ArgumentList.Arguments.FirstOrDefault()?.Expression;

			if (expressionNode == null)
				return;

			SymbolInfo delegateSymbolInfo = semanticModel.GetSymbolInfo(expressionNode, CancellationToken);
			var delegateSymbol = delegateSymbolInfo.Symbol ?? delegateSymbolInfo.CandidateSymbols.FirstOrDefault();

			if (delegateSymbol == null)
				return;

			var delegateNode = expressionNode is AnonymousFunctionExpressionSyntax anonymousMethodOrLambdaNode
				? anonymousMethodOrLambdaNode.Body 
				: delegateSymbol.GetSyntax(CancellationToken);

			if (delegateNode != null)
			{
				var initDelegateInfo = new InitDelegateInfo(graphSymbol, delegateSymbol, delegateNode, _currentDeclarationOrder);
				GraphInitDelegates.Add(initDelegateInfo);
				_currentDeclarationOrder++;
			}
		}
	}
}
