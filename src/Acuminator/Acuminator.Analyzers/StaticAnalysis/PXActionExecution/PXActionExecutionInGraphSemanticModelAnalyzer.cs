﻿#nullable enable

using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXActionExecution
{
    public class PXActionExecutionInGraphSemanticModelAnalyzer : PXGraphAggregatedAnalyzerBase
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                Descriptors.PX1081_PXGraphExecutesActionDuringInitialization,
                Descriptors.PX1082_ActionExecutionInDataViewDelegate);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            Walker walker = new Walker(context, pxContext, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization);

            foreach (GraphInitializerInfo initializer in pxGraph.Initializers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(initializer.Node);
            }

            walker = new Walker(context, pxContext, Descriptors.PX1082_ActionExecutionInDataViewDelegate);

            foreach (DataViewDelegateInfo del in pxGraph.ViewDelegates)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(del.Node);
            }
        }
    }
}
