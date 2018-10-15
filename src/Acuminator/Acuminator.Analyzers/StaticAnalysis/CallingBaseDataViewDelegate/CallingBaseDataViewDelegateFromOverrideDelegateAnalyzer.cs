using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.CallingBaseDataViewDelegate
{
    public class CallingBaseDataViewDelegateFromOverrideDelegateAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1087_PossibleStackOverflowExceptionInBaseViewDelegateInvocation);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var ownDelegatesDictionary = pxGraph.ViewDelegates
                                         .Where(d => pxGraph.Symbol.Equals(d.Symbol.ContainingType))
                                         .ToDictionary(d => d.Symbol.Name, d => d, StringComparer.OrdinalIgnoreCase);
            var ownRelatedViews = pxGraph.Views
                                  .Where(v => ownDelegatesDictionary.ContainsKey(v.Symbol.Name) &&
                                              pxGraph.Symbol.Equals(v.Symbol.ContainingType))
                                  .ToArray();
            var baseNonRedeclaredRelatedViews = new List<DataViewInfo>();

            foreach (var view in pxGraph.Views)
            {
                for (var curView = view; curView != null; curView = curView.Base)
                {
                    if (ownDelegatesDictionary.ContainsKey(curView.Symbol.Name) &&
                        !pxGraph.Symbol.Equals(curView.Symbol.ContainingType) &&
                        ownRelatedViews.All(v => !v.Symbol.Name.Equals(curView.Symbol.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        baseNonRedeclaredRelatedViews.Add(curView);
                    }
                }
            }

            foreach (var viewDelegate in ownDelegatesDictionary.Values)
            {
                var walker = new Walker(context, pxContext, baseNonRedeclaredRelatedViews.ToArray());

                walker.Visit(viewDelegate.Node);
            }
        }

        private class Walker : NestedInvocationWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;
            private readonly DataViewInfo[] _nonRedeclaredBaseViews;

            public Walker(SymbolAnalysisContext context, PXContext pxContext, DataViewInfo[] nonRedeclaredBaseViews)
                : base(context.Compilation, context.CancellationToken)
            {
                pxContext.ThrowOnNull();
                nonRedeclaredBaseViews.ThrowOnNull();

                _context = context;
                _pxContext = pxContext;
                _nonRedeclaredBaseViews = nonRedeclaredBaseViews;
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                ThrowIfCancellationRequested();
                _context.CancellationToken.ThrowIfCancellationRequested();

                var reported = false;
                var symbol = GetSymbol<ISymbol>(node);
                var expressionSymbol = GetSymbol<ISymbol>(node.Expression);

                //Base.PXSelectBaseGenIns.Select()
                if (_pxContext.PXSelectBaseGeneric.Select.Contains(symbol))
                {
                    reported = TryToReport(expressionSymbol, node);
                }
                //Base.PXSelectBaseGenIns.View.Select()
                else if (_pxContext.PXView.Select.Contains(symbol) &&
                         _pxContext.PXSelectBase.View.Equals(expressionSymbol) &&
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

            private bool TryToReport(ISymbol symbol, ExpressionSyntax node)
            {
                if (_nonRedeclaredBaseViews.Any(v => v.Symbol.Equals(symbol)))
                {
                    ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1087_PossibleStackOverflowExceptionInBaseViewDelegateInvocation, node);

                    return true;
                }

                return false;
            }
        }
    }
}
