
using System;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis.ActionHandlerAttributes;
using Acuminator.Analyzers.StaticAnalysis.ActionHandlerReturnType;
using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.CallingBaseActionHandler;
using Acuminator.Analyzers.StaticAnalysis.CallingBaseDataViewDelegate;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInGraphExtension;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions;
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions;
using Acuminator.Analyzers.StaticAnalysis.NoPrimaryViewForPrimaryDac;
using Acuminator.Analyzers.StaticAnalysis.PrivateEventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Analyzers.StaticAnalysis.StaticFieldOrPropertyInGraph;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Analyzers.StaticAnalysis.TypoInViewAndActionHandlerName;
using Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic;
using Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraph
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXGraphAnalyzer : SymbolAnalyzersAggregator<IPXGraphAnalyzer>
	{
		protected override SymbolKind SymbolKind => SymbolKind.NamedType;

		public PXGraphAnalyzer() : this(null,
			new PXGraphCreationInGraphInWrongPlacesGraphAnalyzer(),
			new ConstructorInGraphExtensionAnalyzer(),
			new SavingChangesInGraphSemanticModelAnalyzer(),
			new ChangesInPXCacheDuringPXGraphInitializationAnalyzer(),
			new LongOperationInPXGraphDuringInitializationAnalyzer(),
			new LongOperationInDataViewDelegateAnalyzer(),
			new PXActionExecutionInGraphSemanticModelAnalyzer(),
			new DatabaseQueriesInPXGraphInitializationAnalyzer(),
			new ThrowingExceptionsInLongRunningOperationAnalyzer(),
			new ThrowingExceptionsInActionHandlersAnalyzer(),
			new NoIsActiveMethodForExtensionAnalyzer(),
			new NameConventionEventsInGraphsAndGraphExtensionsAnalyzer(),
			new ThrowingExceptionsInEventHandlersAnalyzer(),
			new CallingBaseDataViewDelegateFromOverrideDelegateAnalyzer(),
			new CallingBaseActionHandlerFromOverrideHandlerAnalyzer(),
			new UiPresentationLogicInActionHandlersAnalyzer(),
			new ViewDeclarationOrderAnalyzer(),
			new NoPrimaryViewForPrimaryDacAnalyzer(),
			new ActionHandlerAttributesAnalyzer(),
			new ActionHandlerReturnTypeAnalyzer(),
			new NonPublicGraphAndDacAndExtensionsAnalyzer(),
			new InvalidPXActionSignatureAnalyzer(),
			new StaticFieldOrPropertyInGraphAnalyzer(),
			new TypoInViewAndActionHandlerNameAnalyzer(),
			new PXOverrideMismatchAnalyzer(),
			new EventHandlerModifierAnalyzer())
		{
		}

		/// <summary>
		/// Constructor for the unit tests.
		/// </summary>
		public PXGraphAnalyzer(CodeAnalysisSettings? settings, params IPXGraphAnalyzer[] innerAnalyzers) : base(settings, innerAnalyzers)
		{
		}

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Symbol is not INamedTypeSymbol type)
				return;

			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			var inferredGraphs = PXGraphSemanticModel.InferModels(pxContext, type, GraphSemanticModelCreationOptions.CollectAll, 
																  cancellation: context.CancellationToken);
			context.CancellationToken.ThrowIfCancellationRequested();

			var graphsEnrichedWithEvents = from graphOrExtension in inferredGraphs
										   where graphOrExtension != null
										   select PXGraphEventSemanticModel.EnrichGraphModelWithEvents(graphOrExtension, context.CancellationToken);

			foreach (var graphOrExtension in graphsEnrichedWithEvents)
			{
				var effectiveAnalyzers = _innerAnalyzers.Where(analyzer => analyzer.ShouldAnalyze(pxContext, graphOrExtension))
														.ToList(capacity: _innerAnalyzers.Length);

				RunAggregatedAnalyzersInParallel(effectiveAnalyzers, context, aggregatedAnalyserAction: analyzerIndex =>
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					var analyzer = effectiveAnalyzers[analyzerIndex];
					analyzer.Analyze(context, pxContext, graphOrExtension);
				},
				parallelOptions);
			}
		}
	}
}
