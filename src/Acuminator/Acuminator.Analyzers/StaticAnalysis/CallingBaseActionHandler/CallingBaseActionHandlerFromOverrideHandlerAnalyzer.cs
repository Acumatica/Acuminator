
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
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

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.GraphType == GraphType.PXGraphExtension;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var redeclaredActionNamesHashSet = graphExtension.Actions
				.Where(action => graphExtension.Symbol.Equals(action.Symbol?.ContainingSymbol) && action.Base != null)
				.Select(action => action.Symbol.Name)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			var redeclaredHandlersWithoutActionsList = graphExtension.ActionHandlers
				.Where(handler => graphExtension.Symbol.Equals(handler.Symbol?.ContainingSymbol) && handler.Base != null &&
								  !redeclaredActionNamesHashSet.Contains(handler.Symbol.Name))
				.ToList();

			var baseHandlersHashSet = redeclaredHandlersWithoutActionsList
				.SelectMany(handler => handler.JustOverridenItems()
											  .Select(baseHandler => baseHandler.Symbol))
				.ToHashSet();

			var baseActionsHashSet = redeclaredHandlersWithoutActionsList
				.SelectMany(handler => graphExtension.ActionsByNames[handler.Symbol.Name]
													 .ThisAndOverridenItems()
													 .Select(action => action.Symbol))
				.ToHashSet();

			var walker = new Walker(context, pxContext, baseActionsHashSet, baseHandlersHashSet);

			foreach (ActionHandlerInfo actionHandler in redeclaredHandlersWithoutActionsList)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				walker.CheckActionHandler(actionHandler);
			}
		}

		private class Walker : NestedInvocationWalker
		{
			private readonly SymbolAnalysisContext _context;
			private readonly HashSet<ISymbol> _baseActions;
			private readonly HashSet<IMethodSymbol> _baseHandlers;

			private ActionHandlerInfo? _currentActionHandler;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, HashSet<ISymbol> baseActions, HashSet<IMethodSymbol> baseHandlers)
				: base(pxContext, context.CancellationToken)
			{
				_baseActions = baseActions.CheckIfNull();
				_baseHandlers = baseHandlers.CheckIfNull();
				_context = context;
			}

			public void CheckActionHandler(ActionHandlerInfo actionHandler)
			{
				try
				{
					_currentActionHandler = actionHandler;
					Visit(_currentActionHandler.Node);
				}
				finally
				{
					_currentActionHandler = null;
				}
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax invocationNode)
			{
				ThrowIfCancellationRequested();

				var methodSymbol = GetSymbol<IMethodSymbol>(invocationNode);
				if (methodSymbol == null)
				{
					return;
				}

				// Case Base.someActionHandler(adapter)
				if (IsDirectCallToBaseActionHandler(invocationNode, methodSymbol))
				{
					ReportDiagnostic(_context.ReportDiagnostic,
						Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation,
						invocationNode);

					return;
				}

				var originalMethodSymbol = methodSymbol.OriginalDefinition?.OverriddenMethod ?? methodSymbol.OriginalDefinition;

				if (originalMethodSymbol == null)
					return;

				// Case Base.SomeAction.Press(adapter)
				if (PxContext.PXAction.Press.Contains(originalMethodSymbol) &&
					invocationNode.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression != null)
				{
					var expressionSymbol = GetSymbol<ISymbol>(memberAccess.Expression);

					if (expressionSymbol != null && _baseActions.Contains(expressionSymbol))
					{
						ReportDiagnostic(_context.ReportDiagnostic,
							Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation,
							invocationNode);

						return;
					}
				}

				base.VisitInvocationExpression(invocationNode);
			}

			/// <summary>
			/// Check if the call is a direct call to the base action handler that looks like this:<br/>
			/// <c>Base.someActionHandler(adapter)</c>
			/// </summary>
			/// <param name="invocationNode">The invocation node.</param>
			/// <param name="calledMethod">The called method.</param>
			/// <returns>
			/// True for a direct call to the base action handler.
			/// </returns>
			private bool IsDirectCallToBaseActionHandler(InvocationExpressionSyntax invocationNode, IMethodSymbol calledMethod)
			{
				if (!_baseHandlers.Contains(calledMethod))
					return false;

				if (_currentActionHandler?.Symbol.IsOverride != true)
					return true;

				// For action handler overrides we must check that this is not an access via base keyword like this:
				// base.someActionHandler(adapter);
				return invocationNode.Expression is not MemberAccessExpressionSyntax memberAccessExpressionNode ||
					   memberAccessExpressionNode.Expression is not BaseExpressionSyntax;
			}
		}
	}
}