using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading;

namespace Acuminator.Analyzers.StaticAnalysis.ActionHandlerReturnType
{
    public class ActionHandlerReturnTypeAnalyzer : PXGraphAggregatedAnalyzerBase
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1013_PXActionHandlerInvalidReturnType);

        public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
        {
            foreach (var actionHandler in pxGraph.DeclaredActionHandlers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (actionHandler.Node == null || actionHandler.Symbol == null)
                {
                    continue;
                }

                CheckActionHandlerReturnType(context, pxContext, actionHandler.Node, actionHandler.Symbol);
            }
        }

        private void CheckActionHandlerReturnType(SymbolAnalysisContext context, PXContext pxContext, MethodDeclarationSyntax node, IMethodSymbol symbol)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (pxContext.SystemTypes.IEnumerable.Equals(symbol.ReturnType))
            {
                return;
            }

            if (!StartsLongOperation(pxContext, node, context.CancellationToken))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(
                Descriptors.PX1013_PXActionHandlerInvalidReturnType,
                node.Identifier.GetLocation());

            context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
        }

        private bool StartsLongOperation(PXContext pxContext, SyntaxNode node, CancellationToken cancellation)
        {
            var walker = new StartLongOperationDelegateWalker(pxContext, cancellation);

            walker.Visit(node);

            return walker.Delegates.Length > 0;
        }
    }
}
