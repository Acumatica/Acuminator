using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.InvalidViewUsageInProcessingDelegate
{
    public class InvalidViewUsageInProcessingDelegateAnalyzer : IPXGraphAnalyzer
    {
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1088_InvalidViewUsageInProcessingDelegate);

        public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!pxGraph.IsProcessing)
            {
                return;
            }

            var processingDelegates = pxGraph.ViewDelegates
                                      .OfType<ProcessingDelegateInfo>();

            foreach (var d in processingDelegates)
            {
                var walker = new Walker(context, pxContext);

                walker.Visit(d.Node);
            }
        }

        private class Walker : NestedInvocationWalker
        {
            public Walker(SymbolAnalysisContext context, PXContext pxContext)
                : base(context.Compilation, context.CancellationToken)
            {
            }
        }
    }
}
