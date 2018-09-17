using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization
{
    public class PXGraphCreationDuringInitializationAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1057_PXGraphCreationDuringInitialization
            );

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            PXGraphCreateInstanceWalker walker = new PXGraphCreateInstanceWalker(context, pxContext);

            foreach(GraphInitializerInfo initializer in pxGraph.Initializers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(initializer.Node);
            }
        }

        private class PXGraphCreateInstanceWalker : CSharpSyntaxWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;

            public PXGraphCreateInstanceWalker(SymbolAnalysisContext context, PXContext pxContext)
            {
                _context = context;
                _pxContext = pxContext;
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                SemanticModel semanticModel = _context.Compilation.GetSemanticModel(node.SyntaxTree);

                if (semanticModel.GetSymbolInfo(node, _context.CancellationToken).Symbol is IMethodSymbol symbol)
                {
                    bool isCreationInstance = _pxContext.PXGraphRelatedMethods.CreateInstance.Contains(symbol.ConstructedFrom);

                    if (isCreationInstance)
                    {
                        _context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1057_PXGraphCreationDuringInitialization, node.GetLocation()));
                    }
                }

                base.VisitMemberAccessExpression(node);
            }
        }
    }
}
