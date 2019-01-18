using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
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
            var walker = new Walker(context, pxContext, pxGraph);

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
			private readonly Dictionary<INamedTypeSymbol, bool> _graphProcessingDictionary = new Dictionary<INamedTypeSymbol, bool>();

            public Walker(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
                : base(context.Compilation, context.CancellationToken)
            {
				pxGraph.ThrowOnNull(nameof(pxGraph));

                _context = context;
                _pxContext = pxContext;
				_graphProcessingDictionary.Add(pxGraph.GraphSymbol, pxGraph.IsProcessing);
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

				var nodeSymbol = semanticModel.GetSymbolInfo(node, CancellationToken).Symbol;
				if (nodeSymbol == null)
				{
					return;
				}

                var isForbiddenSymbol = typeSymbol.InheritsFromOrEqualsGeneric(_pxContext.PXSelectBaseGeneric.Type) &&
                                        !typeSymbol.InheritsFromOrEqualsGeneric(_pxContext.PXProcessingBase.Type) &&
                                        !typeSymbol.InheritsFromOrEqualsGeneric(_pxContext.BQL.PXFilter) &&
                                        !typeSymbol.OriginalDefinition.IsPXSetupBqlCommand(_pxContext);

				if (nodeSymbol.Kind != SymbolKind.Local && isForbiddenSymbol && ContainedInProcessingGraph(nodeSymbol))
				{
					ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1088_InvalidViewUsageInProcessingDelegate, node);
				}
				else
                {
                    base.VisitIdentifierName(node);
                }
            }

			private bool ContainedInProcessingGraph(ISymbol viewSymbol)
			{
				ThrowIfCancellationRequested();

				var containingGraph = viewSymbol.ContainingType;
				if (containingGraph == null)
				{
					return false;
				}

				if (_graphProcessingDictionary.TryGetValue(containingGraph, out bool isProcessing))
				{
					return isProcessing;
				}

				var graph = PXGraphSemanticModel.InferExplicitModel(_pxContext, containingGraph, CancellationToken);
				isProcessing = graph?.IsProcessing ?? false;

				_graphProcessingDictionary.Add(containingGraph, isProcessing);

				return isProcessing;
			}
		}
	}
}
