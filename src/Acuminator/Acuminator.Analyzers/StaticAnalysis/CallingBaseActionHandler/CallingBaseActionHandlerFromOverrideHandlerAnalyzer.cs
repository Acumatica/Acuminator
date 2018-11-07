using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.CallingBaseActionHandler
{
    public class CallingBaseActionHandlerFromOverrideHandlerAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (pxGraph.Type != GraphType.PXGraphExtension)
            {
                return;
            }

            var redeclaredActionNamesHashSet = pxGraph.Actions
                .Where(a => pxGraph.Symbol.Equals(a.Symbol?.ContainingSymbol) && a.Base != null)
                .Select(a => a.Symbol.Name)
                .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            var redeclaredHandlersWithoutActionsList = pxGraph.ActionHandlers
                .Where(h => pxGraph.Symbol.Equals(h.Symbol?.ContainingSymbol) && h.Base != null)
                .Where(h => !redeclaredActionNamesHashSet.Contains(h.Symbol.Name))
                .ToList();

            var baseHandlersHashSet = redeclaredHandlersWithoutActionsList
                .Select(h => h.Base.Symbol)
                .ToImmutableHashSet();

            var baseActionsHashSet = redeclaredHandlersWithoutActionsList
                .Select(h => pxGraph.ActionsByNames[h.Symbol.Name].Symbol)
                .ToImmutableHashSet();

            var walker = new Walker(context, pxContext, baseActionsHashSet, baseHandlersHashSet);

            foreach (var actionHandler in redeclaredHandlersWithoutActionsList)
            {
                walker.Visit(actionHandler.Node);
            }
        }

        private class Walker : NestedInvocationWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;
            private readonly ImmutableHashSet<ISymbol> _baseActions;
            private readonly ImmutableHashSet<IMethodSymbol> _baseHandlers;

            public Walker(SymbolAnalysisContext context, PXContext pxContext, ImmutableHashSet<ISymbol> baseActions, ImmutableHashSet<IMethodSymbol> baseHandlers)
                : base(context.Compilation, context.CancellationToken)
            {
                pxContext.ThrowOnNull(nameof(pxContext));
                baseActions.ThrowOnNull(nameof(baseActions));
                baseHandlers.ThrowOnNull(nameof(baseHandlers));

                _pxContext = pxContext;
                _baseActions = baseActions;
                _baseHandlers = baseHandlers;
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
                    ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation, node);
                    return;
                }

                var originalMethodSymbol = methodSymbol.OriginalDefinition?.OverriddenMethod ?? methodSymbol.OriginalDefinition;

                // Case Base.SomeAction.Press(adapter)
                if (_pxContext.PXAction.Press.Contains(originalMethodSymbol) &&
                    node.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression != null)
                {
                    var expressionSymbol = GetSymbol<ISymbol>(memberAccess.Expression);

                    if (_baseActions.Contains(expressionSymbol))
                    {
                        ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation, node);
                        return;
                    }
                }

                base.VisitInvocationExpression(node);
            }
        }
    }
}
