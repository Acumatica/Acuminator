using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PXGraphAnalyzer : PXDiagnosticAnalyzer
    {
        private readonly ImmutableArray<IPXGraphAnalyzer> _innerAnalyzers;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public PXGraphAnalyzer()// : this()
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
            compilationStartContext.RegisterSymbolAction(t => AnalyzeType(t, pxContext), SymbolKind.NamedType);
        }

        private void AnalyzeType(SymbolAnalysisContext context, PXContext pxContext)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (!(context.Symbol is INamedTypeSymbol type))
                return;

            PXGraphSemanticModel pxGrpah = PXGraphSemanticModel.GetModel(context, pxContext, type);
            if (pxGrpah == null)
                return;

            foreach(IPXGraphAnalyzer innerAnalyzer in _innerAnalyzers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                innerAnalyzer.Analyze(context, pxContext, pxGrpah);
            }
        }
    }
}
