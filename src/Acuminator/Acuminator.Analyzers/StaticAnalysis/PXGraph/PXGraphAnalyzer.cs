using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PXGraphAnalyzer : SymbolAnalyzersAggregator<IPXGraphAnalyzer>
    {
        protected override SymbolKind SymbolKind => SymbolKind.NamedType;

        public PXGraphAnalyzer() : this(null,
            new PXGraphCreationInGraphSemanticModelAnalyzer(),
            new SavingChangesInGraphSemanticModelAnalyzer(),
            new ChangesInPXCacheDuringPXGraphInitializationAnalyzer(),
            new LongOperationInPXGraphDuringInitializationAnalyzer(),
            new LongOperationInDataViewDelegateAnalyzer(),
            new PXActionExecutionInGraphSemanticModelAnalyzer())
        {
        }

        /// <summary>
        /// Constructor for the unit tests.
        /// </summary>
        public PXGraphAnalyzer(CodeAnalysisSettings codeAnalysisSettings, params IPXGraphAnalyzer[] innerAnalyzers)
            : base(codeAnalysisSettings, innerAnalyzers)
        {
        }

        protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!(context.Symbol is INamedTypeSymbol type))
                return;

            IEnumerable<PXGraphSemanticModel> graphs = PXGraphSemanticModel.InferModels(pxContext, type, context.CancellationToken);

            foreach (IPXGraphAnalyzer innerAnalyzer in _innerAnalyzers)
            {
                foreach (PXGraphSemanticModel g in graphs)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    innerAnalyzer.Analyze(context, pxContext, settings, g);
                }
            }
        }
    }
}
