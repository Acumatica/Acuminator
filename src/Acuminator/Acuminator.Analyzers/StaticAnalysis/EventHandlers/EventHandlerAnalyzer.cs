using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Analyzers.StaticAnalysis.RaiseExceptionHandling;
using Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EventHandlerAnalyzer : SymbolAnalyzersAggregator<IEventHandlerAnalyzer>
	{
        protected override SymbolKind SymbolKind => SymbolKind.Method;

		public EventHandlerAnalyzer() : this(null,
			// can be replaced with DI from ServiceLocator if DI-container is used
			new DatabaseQueriesInRowSelectingAnalyzer(),
			new SavingChangesInEventHandlersAnalyzer(),
			new ChangesInPXCacheInEventHandlersAnalyzer(),
			new PXGraphCreateInstanceInEventHandlersAnalyzer(),
			new LongOperationInEventHandlersAnalyzer(),
			new RowChangesInEventHandlersAnalyzer(),
			new DatabaseQueriesInRowSelectedAnalyzer(),
			new UiPresentationLogicInEventHandlersAnalyzer(),
			new PXActionExecutionInEventHandlersAnalyzer(),
			new ThrowingExceptionsInEventHandlersAnalyzer(),
			new RaiseExceptionHandlingInEventHandlersAnalyzer())
		{
		}

		/// <summary>
		/// Constructor for the unit tests.
		/// </summary>
		public EventHandlerAnalyzer(CodeAnalysisSettings settings, params IEventHandlerAnalyzer[] innerAnalyzers)
            : base(settings, innerAnalyzers)
		{
		}

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!(context.Symbol is IMethodSymbol methodSymbol))
				return;
		
			EventType eventType = methodSymbol.GetEventHandlerType(pxContext);

			if (eventType == EventType.None)
				return;

			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			Parallel.ForEach(_innerAnalyzers, parallelOptions, innerAnalyzer =>
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (innerAnalyzer.ShouldAnalyze(pxContext, eventType))
				{
					innerAnalyzer.Analyze(context, pxContext, eventType);
				}
			});
		}

		private void AnalyzeLambda(OperationAnalysisContext context, PXContext pxContext)
		{
			if (context.Operation is ILambdaExpression lambdaExpression)
			{
				var symbolAnalysisContext =
					new SymbolAnalysisContext(lambdaExpression.Signature, context.Compilation, context.Options,
											  context.ReportDiagnostic, d => true, // this check is covered inside context.ReportDiagnostic
											  context.CancellationToken);

				AnalyzeSymbol(symbolAnalysisContext, pxContext);
			}
		}
	}
}
