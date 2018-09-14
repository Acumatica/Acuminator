using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            new PXGraphCreationDuringInitializationAnalyzer())
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
            compilationStartContext.RegisterSyntaxNodeAction(context => AnalyzeGraph(context, pxContext), SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeGraph(SyntaxNodeAnalysisContext context, PXContext pxContext)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!(context.Node is ClassDeclarationSyntax node))
                return;

            INamedTypeSymbol type = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);
            if (type == null)
                return;

            IEnumerable<PXGraphSemanticModel> graphs = PXGraphSemanticModel.InferModels(pxContext, type, context.SemanticModel, context.CancellationToken);

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
