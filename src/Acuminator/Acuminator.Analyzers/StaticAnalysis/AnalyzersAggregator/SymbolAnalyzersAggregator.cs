using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator
{
    public abstract class SymbolAnalyzersAggregator<T> : PXDiagnosticAnalyzer
        where T : ISymbolAnalyzer
    {
        protected readonly ImmutableArray<T> _innerAnalyzers;

        protected abstract SymbolKind SymbolKind { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        protected SymbolAnalyzersAggregator(CodeAnalysisSettings settings, params T[] innerAnalyzers) : base(settings)
        {
            _innerAnalyzers = ImmutableArray.CreateRange(innerAnalyzers);
            SupportedDiagnostics = ImmutableArray.CreateRange(innerAnalyzers.SelectMany(a => a.SupportedDiagnostics));
        }

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSymbolAction(c => AnalyzeSymbolHandleAggregateException(c, pxContext), SymbolKind);
            // TODO: Enable this operation action after migration to Roslyn v2
            //compilationStartContext.RegisterOperationAction(c => AnalyzeLambda(c, pxContext, codeAnalysisSettings), OperationKind.LambdaExpression);
        }

		private void AnalyzeSymbolHandleAggregateException(SymbolAnalysisContext context, PXContext pxContext)
		{
			try
			{
				AnalyzeSymbol(context, pxContext);
			}
			catch (AggregateException e)
			{
				var operationCanceledException = e.Flatten().InnerExceptions
					.OfType<OperationCanceledException>()
					.FirstOrDefault();

				if (operationCanceledException != null)
				{
					throw operationCanceledException;
				}

				throw;
			}
		}

		protected abstract void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext);
	}
}
