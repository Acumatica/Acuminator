using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Analyzers.StaticAnalysis.SavingChanges
{
    public class SavingChangesDuringPXGraphInitializationAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1058_PXGraphSavingChangesDuringInitialization
            );

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            foreach (GraphInitializerInfo initializer in pxGraph.Initializers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                CheckSavingChanges(context, pxContext, initializer);
            }
        }

        private void CheckSavingChanges(SymbolAnalysisContext context, PXContext pxContext, GraphInitializerInfo initializer)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (initializer.Node == null)
                return;

            SaveChangesWalker walker = new SaveChangesWalker(context, pxContext);

            walker.Visit(initializer.Node);
        }

        private class SaveChangesWalker : NestedInvocationWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;

            public SaveChangesWalker(SymbolAnalysisContext context, PXContext pxContext)
                : base(context.Compilation, context.CancellationToken)
            {
                _context = context;
                _pxContext = pxContext;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                IMethodSymbol symbol = GetSymbol<IMethodSymbol>(node);
                SemanticModel semanticModel = GetSemanticModel(node.SyntaxTree);

                if (symbol != null && SaveOperationHelper.GetSaveOperationKind(symbol, node, semanticModel, _pxContext) != SaveOperationKind.None)
                {
                    ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1058_PXGraphSavingChangesDuringInitialization, node);
                }
                else
                {
                    base.VisitInvocationExpression(node);
                }
            }
        }
    }
}
