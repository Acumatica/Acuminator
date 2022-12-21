#nullable enable

using System;
using System.Collections.Generic;
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

namespace Acuminator.Analyzers.StaticAnalysis.CallingBaseActionHandler
{
	public class CallingBaseActionHandlerFromOverrideHandlerAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.Type == GraphType.PXGraphExtension;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var redeclaredActionNamesHashSet = graphExtension.Actions
				.Where(a => graphExtension.Symbol.Equals(a.Symbol?.ContainingSymbol) && a.Base != null)
				.Select(a => a.Symbol.Name)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			var redeclaredHandlersWithoutActionsList = graphExtension.ActionHandlers
				.Where(h => graphExtension.Symbol.Equals(h.Symbol?.ContainingSymbol) && h.Base != null &&
							!redeclaredActionNamesHashSet.Contains(h.Symbol.Name))
				.ToList();

			var baseHandlersHashSet = redeclaredHandlersWithoutActionsList
				.Select(h => h.Base.Symbol)
				.ToHashSet();

			var baseActionsHashSet = redeclaredHandlersWithoutActionsList
				.Select(h => graphExtension.ActionsByNames[h.Symbol.Name].Symbol)
				.ToHashSet();

			var walker = new Walker(context, pxContext, baseActionsHashSet, baseHandlersHashSet);

			foreach (var actionHandler in redeclaredHandlersWithoutActionsList)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				walker.Visit(actionHandler.Node);
			}
		}

		private class Walker : NestedInvocationWalker
		{
			private readonly SymbolAnalysisContext _context;
			private readonly HashSet<ISymbol> _baseActions;
			private readonly HashSet<IMethodSymbol> _baseHandlers;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, HashSet<ISymbol> baseActions, HashSet<IMethodSymbol> baseHandlers)
				: base(pxContext, context.CancellationToken)
			{
				_baseActions = baseActions.CheckIfNull(nameof(baseActions));
				_baseHandlers = baseHandlers.CheckIfNull(nameof(baseHandlers));
				_context = context;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				ThrowIfCancellationRequested();

				var methodSymbol = GetSymbol<IMethodSymbol>(node);
				if (methodSymbol == null)
				{
					return;
				}

				// Case Base.someActionHandler(adapter)
				if (_baseHandlers.Contains(methodSymbol))
				{
					ReportDiagnostic(_context.ReportDiagnostic,
						Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation,
						node);

					return;
				}

				var originalMethodSymbol = methodSymbol.OriginalDefinition?.OverriddenMethod ?? methodSymbol.OriginalDefinition;

				// Case Base.SomeAction.Press(adapter)
				if (PxContext.PXAction.Press.Contains(originalMethodSymbol) &&
					node.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression != null)
				{
					var expressionSymbol = GetSymbol<ISymbol>(memberAccess.Expression);

					if (expressionSymbol != null && _baseActions.Contains(expressionSymbol))
					{
						ReportDiagnostic(_context.ReportDiagnostic,
							Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation,
							node);

						return;
					}
				}

				base.VisitInvocationExpression(node);
			}
		}
	}
}