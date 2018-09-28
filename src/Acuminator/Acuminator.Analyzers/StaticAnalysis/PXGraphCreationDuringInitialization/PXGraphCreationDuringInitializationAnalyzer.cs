using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
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

        private class PXGraphCreateInstanceWalker : NestedInvocationWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;

            public PXGraphCreateInstanceWalker(SymbolAnalysisContext context, PXContext pxContext)
                : base(context.Compilation, context.CancellationToken)
            {
                _context = context;
                _pxContext = pxContext;
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                IMethodSymbol symbol = GetSymbol<IMethodSymbol>(node);

                if (symbol != null && _pxContext.PXGraphRelatedMethods.CreateInstance.Contains(symbol.ConstructedFrom))
                {
                    ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1057_PXGraphCreationDuringInitialization, node);
                }
                else
                {
                    base.VisitMemberAccessExpression(node);
                }
            }
        }
    }
}
