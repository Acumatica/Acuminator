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
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Analyzers.StaticAnalysis.NonPublicExtensions;
using Acuminator.Analyzers.StaticAnalysis.NoPrimaryViewForPrimaryDac;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Analyzers.StaticAnalysis.TypoInViewDelegateName;
using Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic;
using Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder;
using Acuminator.Utilities;
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
            new ThrowingExceptionsInEventHandlersAnalyzer(),
            new CallingBaseDataViewDelegateFromOverrideDelegateAnalyzer(),
            new CallingBaseActionHandlerFromOverrideHandlerAnalyzer(),
            new UiPresentationLogicInActionHandlersAnalyzer(),
			new ViewDeclarationOrderAnalyzer(),
			new NoPrimaryViewForPrimaryDacAnalyzer(),
			new ActionHandlerAttributesAnalyzer(),
            new ActionHandlerReturnTypeAnalyzer(),
			new NonPublicGraphAndDacExtensionAnalyzer(),
			new NoIsActiveMethodForExtensionAnalyzer(),
			new InvalidPXActionSignatureAnalyzer(),
			new TypoInViewDelegateNameAnalyzer())
        {
        }

        /// <summary>
        /// Constructor for the unit tests.
        /// </summary>
        public PXGraphAnalyzer(CodeAnalysisSettings settings, params IPXGraphAnalyzer[] innerAnalyzers) : base(settings, innerAnalyzers)
        {
        }

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!(context.Symbol is INamedTypeSymbol type))
			{
				return;
			}

			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			var inferredGraphs = PXGraphSemanticModel.InferModels(pxContext, type, GraphSemanticModelCreationOptions.CollectAll, context.CancellationToken);
			
			foreach (var graph in inferredGraphs)
			{
				Parallel.ForEach(_innerAnalyzers, parallelOptions, innerAnalyzer =>
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					if (innerAnalyzer.ShouldAnalyze(pxContext, graph))
					{
						innerAnalyzer.Analyze(context, pxContext, graph);
					}
				});
			}
		}
    }
}
