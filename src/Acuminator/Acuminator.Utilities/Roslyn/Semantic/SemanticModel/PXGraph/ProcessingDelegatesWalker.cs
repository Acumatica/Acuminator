#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Walkers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	internal class ProcessingDelegatesWalker : DelegatesWalkerBase
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

			if (viewSymbol == null)
				return;

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

		private void AnalyzeSetParametersDelegate(string viewName, ArgumentListSyntax? argumentList)
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

			var parametersDelegateInfo = GetDelegateInfo(handlerNode);

			if (parametersDelegateInfo != null)
				ParametersDelegateListByView[viewName].Add(parametersDelegateInfo);
		}

		private void AnalyzeSetProcessDelegate(string viewName, ArgumentListSyntax? argumentList)
		{
			ThrowIfCancellationRequested();

			ExpressionSyntax? handlerNode = argumentList?.Arguments.First()?.Expression;
			if (handlerNode == null)
			{
				return;
			}

			if (!ProcessDelegateListByView.ContainsKey(viewName))
			{
				ProcessDelegateListByView.Add(viewName, new List<ProcessingDelegateInfo>());
			}

			var processDelegateInfo = GetDelegateInfo(handlerNode);

			if (processDelegateInfo != null)
				ProcessDelegateListByView[viewName].Add(processDelegateInfo);

			if (argumentList!.Arguments.Count == 1)
				return;

			var finallyHandlerNode = argumentList.Arguments[1]?.Expression;

			if (finallyHandlerNode == null)
				return;

			if (!FinallyProcessDelegateListByView.ContainsKey(viewName))
			{
				FinallyProcessDelegateListByView.Add(viewName, new List<ProcessingDelegateInfo>());
			}

			var finallyHandlerInfo = GetDelegateInfo(finallyHandlerNode);

			if (finallyHandlerInfo != null)
				FinallyProcessDelegateListByView[viewName].Add(finallyHandlerInfo);
		}

		private ProcessingDelegateInfo? GetDelegateInfo(ExpressionSyntax handlerNode)
		{
			ThrowIfCancellationRequested();

			var (delegateSymbol, delegateNode) = GetDelegateSymbolAndNode(handlerNode);

			if (delegateSymbol == null || delegateNode == null)  // Skip analysis for unrecognized arguments
				return null;

			var processingDelegateInfo = new ProcessingDelegateInfo(delegateNode, delegateSymbol, _currentDeclarationOrder);

			_currentDeclarationOrder++;
			return processingDelegateInfo;
		}
	}
}
