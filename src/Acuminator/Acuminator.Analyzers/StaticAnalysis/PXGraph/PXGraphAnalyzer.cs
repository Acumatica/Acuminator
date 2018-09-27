﻿using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder;
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
    public class PXGraphAnalyzer : PXDiagnosticAnalyzer
    {
        private readonly ImmutableArray<IPXGraphAnalyzer> _innerAnalyzers;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public PXGraphAnalyzer() : this(
            new PXGraphCreationDuringInitializationAnalyzer(),
            new SavingChangesDuringPXGraphInitializationAnalyzer(),
            new ChangesInPXCacheDuringPXGraphInitializationAnalyzer(),
            new LongOperationInPXGraphDuringInitializationAnalyzer(),
            new LongOperationInDataViewDelegateAnalyzer(),
            new ViewDeclarationOrderAnalyzer())
        {
        }

        /// <summary>
        /// Constructor for the unit tests.
        /// </summary>
        public PXGraphAnalyzer(params IPXGraphAnalyzer[] innerAnalyzers)
        {
            _innerAnalyzers = ImmutableArray.CreateRange(innerAnalyzers);
            SupportedDiagnostics = ImmutableArray.CreateRange(innerAnalyzers.SelectMany(a => a.SupportedDiagnostics));
        }

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSymbolAction(context => AnalyzeGraph(context, pxContext), SymbolKind.NamedType);
        }

        private void AnalyzeGraph(SymbolAnalysisContext context, PXContext pxContext)
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
                    innerAnalyzer.Analyze(context, pxContext, g);
                }
            }
        }
    }
}
