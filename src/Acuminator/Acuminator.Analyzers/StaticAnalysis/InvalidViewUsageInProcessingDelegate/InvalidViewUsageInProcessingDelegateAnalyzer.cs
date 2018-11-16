using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.InvalidViewUsageInProcessingDelegate
{
    public class InvalidViewUsageInProcessingDelegateAnalyzer : PXGraphAggregatedAnalyzerBase
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1088_InvalidViewUsageInProcessingDelegate);

		public override bool ShouldAnalyze(PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel graph) =>
			graph.IsProcessing;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings,
									 PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var processingViews = pxGraph.Views.Where(v => v.IsProcessing);
            var walker = new Walker(context, pxContext);

            foreach (var view in processingViews)
            {
                foreach (var d in view.ParametersDelegates)
                {
                    walker.Visit(d.Node);
                }

                foreach (var d in view.ProcessDelegates)
                {
                    walker.Visit(d.Node);
                }

                foreach (var d in view.FinallyProcessDelegates)
                {
                    walker.Visit(d.Node);
                }
            }
        }

        private class Walker : NestedInvocationWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;

            public Walker(SymbolAnalysisContext context, PXContext pxContext)
                : base(context.Compilation, context.CancellationToken)
            {
                _context = context;
                _pxContext = pxContext;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                ThrowIfCancellationRequested();

                var semanticModel = GetSemanticModel(node.SyntaxTree);
                if (semanticModel == null)
                {
                    return;
                }

                var typeSymbol = semanticModel.GetTypeInfo(node, CancellationToken).Type;
                if (typeSymbol == null)
                {
                    return;
                }

                var isForbiddenSymbol = typeSymbol.InheritsFromOrEqualsGeneric(_pxContext.PXSelectBaseGeneric.Type) &&
                                        !typeSymbol.InheritsFromOrEqualsGeneric(_pxContext.PXProcessingBase.Type) &&
                                        !typeSymbol.InheritsFromOrEqualsGeneric(_pxContext.BQL.PXFilter) &&
                                        !typeSymbol.OriginalDefinition.IsPXSetupBqlCommand(_pxContext);
                if (isForbiddenSymbol)
                {
                    ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1088_InvalidViewUsageInProcessingDelegate, node);
                }
                else
                {
                    base.VisitIdentifierName(node);
                }
            }
        }
    }
}
