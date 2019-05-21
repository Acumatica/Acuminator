using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	internal class ProcessingDelegatesWalker : NestedInvocationWalker
	{
		private readonly PXContext _pxContext;
		private int _currentDeclarationOrder;
		private readonly ImmutableHashSet<ISymbol> _processingViewSymbols;

		public Dictionary<string, List<ProcessingDelegateInfo>> ParametersDelegateListByView { get; } =
			new Dictionary<string, List<ProcessingDelegateInfo>>();
		public Dictionary<string, List<ProcessingDelegateInfo>> ProcessDelegateListByView { get; } =
			new Dictionary<string, List<ProcessingDelegateInfo>>();
		public Dictionary<string, List<ProcessingDelegateInfo>> FinallyProcessDelegateListByView { get; } =
			new Dictionary<string, List<ProcessingDelegateInfo>>();

		public ProcessingDelegatesWalker(PXContext pxContext, ImmutableHashSet<ISymbol> processingViewSymbols, CancellationToken cancellation)
			: base(pxContext.Compilation, cancellation, pxContext.CodeAnalysisSettings)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			processingViewSymbols.ThrowOnNull(nameof(processingViewSymbols));

			_pxContext = pxContext;
			_processingViewSymbols = processingViewSymbols;
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			if (node.ArgumentList == null || !(node.Expression is MemberAccessExpressionSyntax memberAccess))
			{
				return;
			}

			var viewSymbol = GetSymbol<ISymbol>(memberAccess.Expression);
			var isProcessingView = _processingViewSymbols.Contains(viewSymbol);

			if (!isProcessingView)
			{
				return;
			}

			var viewName = viewSymbol.Name;
			var methodSymbol = GetSymbol<IMethodSymbol>(memberAccess.Name);

			if (methodSymbol == null)
			{
				return;
			}

			var isSetParametersDelegate = _pxContext.PXProcessingBase.SetParametersDelegate.Equals(methodSymbol.OriginalDefinition);

			if (isSetParametersDelegate)
			{
				AnalyzeSetParametersDelegate(viewName, node.ArgumentList);
			}
			else
			{
				var isSetProcessDelegate = _pxContext.PXProcessingBase.SetProcessDelegate.Contains(methodSymbol.OriginalDefinition);

				if (isSetProcessDelegate)
				{
					AnalyzeSetProcessDelegate(viewName, node.ArgumentList);
				}
			}

			base.VisitInvocationExpression(node);
		}

		private void AnalyzeSetParametersDelegate(string viewName, ArgumentListSyntax argumentList)
		{
			ThrowIfCancellationRequested();

			var handlerNode = argumentList?.Arguments.First()?.Expression;
			if (handlerNode == null)
			{
				return;
			}

			if (!ParametersDelegateListByView.ContainsKey(viewName))
			{
				ParametersDelegateListByView.Add(viewName, new List<ProcessingDelegateInfo>());
			}

			ParametersDelegateListByView[viewName].Add(GetDelegateInfo(handlerNode));
		}

		private void AnalyzeSetProcessDelegate(string viewName, ArgumentListSyntax argumentList)
		{
			ThrowIfCancellationRequested();

			var handlerNode = argumentList?.Arguments.First()?.Expression;
			if (handlerNode == null)
			{
				return;
			}

			if (!ProcessDelegateListByView.ContainsKey(viewName))
			{
				ProcessDelegateListByView.Add(viewName, new List<ProcessingDelegateInfo>());
			}

			ProcessDelegateListByView[viewName].Add(GetDelegateInfo(handlerNode));

			if (argumentList.Arguments.Count == 1)
			{
				return;
			}

			var finallyHandlerNode = argumentList.Arguments[1]?.Expression;
			if (finallyHandlerNode == null)
			{
				return;
			}

			if (!FinallyProcessDelegateListByView.ContainsKey(viewName))
			{
				FinallyProcessDelegateListByView.Add(viewName, new List<ProcessingDelegateInfo>());
			}

			FinallyProcessDelegateListByView[viewName].Add(GetDelegateInfo(finallyHandlerNode));
		}

		private ProcessingDelegateInfo GetDelegateInfo(ExpressionSyntax handlerNode)
		{
			ThrowIfCancellationRequested();

			ISymbol delegateSymbol;
			SyntaxNode delegateNode;

			if (handlerNode is AnonymousFunctionExpressionSyntax anonymousFunction)
			{
				delegateNode = anonymousFunction.Body;
				delegateSymbol = GetSemanticModel(delegateNode.SyntaxTree)
									?.GetSymbolInfo(anonymousFunction, CancellationToken).Symbol;
			}
			else
			{
				delegateSymbol = GetSymbol<ISymbol>(handlerNode);
				delegateNode = delegateSymbol.DeclaringSyntaxReferences.FirstOrDefault()
																	  ?.GetSyntax(CancellationToken);
			}

			var processingDelegateInfo = new ProcessingDelegateInfo(delegateNode, delegateSymbol, _currentDeclarationOrder);
			_currentDeclarationOrder++;
			return processingDelegateInfo;
		}
	}
}
