﻿#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.CallingBaseDataViewDelegate
{
	public class CallingBaseDataViewDelegateFromOverrideDelegateAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1087_CausingStackOverflowExceptionInBaseViewDelegateInvocation);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel pxGraph)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var ownDelegatesDictionary = pxGraph.ViewDelegates
										 .Where(d => pxGraph.Symbol.Equals(d.Symbol.ContainingType))
										 .ToDictionary(d => d.Symbol.Name, d => d, StringComparer.OrdinalIgnoreCase);
			var ownRelatedViewsHashSet = pxGraph.Views
										 .Where(v => ownDelegatesDictionary.ContainsKey(v.Symbol.Name) &&
													 pxGraph.Symbol.Equals(v.Symbol.ContainingType))
										 .Select(v => v.Symbol.Name)
										 .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
			var baseNonRedeclaredRelatedViewsBuilder = ImmutableHashSet<ISymbol>.Empty.ToBuilder();

			foreach (var view in pxGraph.Views)
			{
				for (var curView = view; curView != null; curView = curView.Base)
				{
					if (ownDelegatesDictionary.ContainsKey(curView.Symbol.Name) &&
						!pxGraph.Symbol.Equals(curView.Symbol.ContainingType) &&
						!ownRelatedViewsHashSet.Contains(curView.Symbol.Name))
					{
						baseNonRedeclaredRelatedViewsBuilder.Add(curView.Symbol);
					}
				}
			}

			var baseNonRedeclaredRelatedViewsHashSet = baseNonRedeclaredRelatedViewsBuilder.ToImmutable();

			foreach (var viewDelegate in ownDelegatesDictionary.Values)
			{
				var walker = new Walker(context, pxContext, baseNonRedeclaredRelatedViewsHashSet);

				walker.Visit(viewDelegate.Node);
			}
		}

		private class Walker : NestedInvocationWalker
		{
			private readonly SymbolAnalysisContext _context;
			private readonly ImmutableHashSet<ISymbol> _nonRedeclaredBaseViews;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, ImmutableHashSet<ISymbol> nonRedeclaredBaseViews)
				: base(pxContext, context.CancellationToken)
			{
				_context = context;
				_nonRedeclaredBaseViews = nonRedeclaredBaseViews.CheckIfNull();
			}

			public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
			{
				ThrowIfCancellationRequested();

				var reported = false;
				var symbol = GetSymbol<ISymbol>(node);
				var methodSymbol = symbol as IMethodSymbol;

				if (methodSymbol != null)
				{
					methodSymbol = methodSymbol.OriginalDefinition?.OverriddenMethod ?? methodSymbol.OriginalDefinition;
				}

				var expressionSymbol = GetSymbol<ISymbol>(node.Expression);

				// Case Base.PXSelectBaseGenIns.Select()
				if (PxContext.PXSelectBaseGeneric.Select.Contains(methodSymbol))
				{
					reported = TryToReport(expressionSymbol, node);
				}
				// Case Base.PXSelectBaseGenIns.View.Select()
				else if (PxContext.PXView.Select.Contains(symbol) &&
						 PxContext.PXSelectBase.View.Equals(expressionSymbol) &&
						 node.Expression is MemberAccessExpressionSyntax expressionNode)
				{
					var innerExpressionSymbol = GetSymbol<ISymbol>(expressionNode.Expression);
					reported = TryToReport(innerExpressionSymbol, node);
				}

				if (!reported)
				{
					base.VisitMemberAccessExpression(node);
				}
			}

			private bool TryToReport(ISymbol? symbol, ExpressionSyntax node)
			{
				ThrowIfCancellationRequested();

				if (symbol != null && _nonRedeclaredBaseViews.Contains(symbol))
				{
					ReportDiagnostic(_context.ReportDiagnostic,
						Descriptors.PX1087_CausingStackOverflowExceptionInBaseViewDelegateInvocation,
						node);

					return true;
				}

				return false;
			}
		}
	}
}
