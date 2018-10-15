using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using CommonServiceLocator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator
{
    public abstract class SymbolAnalyzersAggregator<T> : PXDiagnosticAnalyzer
        where T : ISymbolAnalyzer
    {
        private readonly CodeAnalysisSettings _settings;
        protected readonly ImmutableArray<T> _innerAnalyzers;

        protected abstract SymbolKind SymbolKind { get; }
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        protected SymbolAnalyzersAggregator(CodeAnalysisSettings settings, params T[] innerAnalyzers)
        {
            _settings = settings;
            _innerAnalyzers = ImmutableArray.CreateRange(innerAnalyzers);
            SupportedDiagnostics = ImmutableArray.CreateRange(innerAnalyzers.SelectMany(a => a.SupportedDiagnostics));
        }

        protected abstract void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            var codeAnalysisSettings = GetCodeAnalysisSettings();

            compilationStartContext.RegisterSymbolAction(c => AnalyzeSymbol(c, pxContext, codeAnalysisSettings), SymbolKind);
            // TODO: Enable this operation action after migration to Roslyn v2
            //compilationStartContext.RegisterOperationAction(c => AnalyzeLambda(c, pxContext, codeAnalysisSettings), OperationKind.LambdaExpression);
        }

        private CodeAnalysisSettings GetCodeAnalysisSettings()
        {
            if (_settings != null)
                return _settings; // for unit tests

            CodeAnalysisSettings settings = null;

            try
            {
                if (ServiceLocator.IsLocationProviderSet)
                    settings = ServiceLocator.Current.GetInstance<CodeAnalysisSettings>();
            }
            catch
            {
                // TODO: log the exception
            }

            return settings ?? CodeAnalysisSettings.Default;
        }
    }
}
