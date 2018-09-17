using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphSavingChangesDuringInitialization
{
    public class PXGraphSavingChangesDuringInitializationAnalyzer : IPXGraphAnalyzer
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

            SaveChangesWalker walker = new SaveChangesWalker(context, pxContext);

            walker.Visit(initializer.Node);
        }

        private class SaveChangesWalker : CSharpSyntaxWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;

            public SaveChangesWalker(SymbolAnalysisContext context, PXContext pxContext)
            {
                _context = context;
                _pxContext = pxContext;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                SemanticModel semanticModel = _context.Compilation.GetSemanticModel(node.SyntaxTree);

                if (semanticModel != null && semanticModel.GetSymbolInfo(node, _context.CancellationToken).Symbol is IMethodSymbol method)
                {
                    SaveOperationKind saveOperation = SaveOperationHelper.GetSaveOperationKind(method, node, semanticModel, _pxContext);

                    if (saveOperation != SaveOperationKind.None)
                    {
                        _context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1058_PXGraphSavingChangesDuringInitialization, node.GetLocation()));
                    }
                }

                base.VisitInvocationExpression(node);
            }
        }
    }
}
