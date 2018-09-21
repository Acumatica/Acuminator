using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance;
using Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using CommonServiceLocator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EventHandlerAnalyzer : PXDiagnosticAnalyzer
	{
		private readonly CodeAnalysisSettings _codeAnalysisSettings;
		private readonly ImmutableArray<IEventHandlerAnalyzer> _innerAnalyzers;

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public EventHandlerAnalyzer() : this(null,
			// can be replaced with DI from ServiceLocator if DI-container is used
			new DatabaseQueriesInRowSelectingAnalyzer(),
			new SavingChangesInEventHandlersAnalyzer(),
			new ChangesInPXCacheInEventHandlersAnalyzer(),
			new PXGraphCreateInstanceInEventHandlersAnalyzer(),
			new LongOperationInEventHandlersAnalyzer(),
			new RowChangesInEventHandlersAnalyzer(),
			new DatabaseQueriesInRowSelectedAnalyzer())
		{
		}

		/// <summary>
		/// Constructor for the unit tests.
		/// </summary>
		public EventHandlerAnalyzer(CodeAnalysisSettings codeAnalysisSettings, params IEventHandlerAnalyzer[] innerAnalyzers)
		{
			_codeAnalysisSettings = codeAnalysisSettings;
			_innerAnalyzers = ImmutableArray.CreateRange(innerAnalyzers);
			SupportedDiagnostics = ImmutableArray.CreateRange(innerAnalyzers.SelectMany(a => a.SupportedDiagnostics));
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			var codeAnalysisSettings = GetCodeAnalysisSettings();

			compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext, codeAnalysisSettings), SymbolKind.Method);
			// TODO: Enable this operation action after migration to Roslyn v2
			//compilationStartContext.RegisterOperationAction(c => AnalyzeLambda(c, pxContext, codeAnalysisSettings), OperationKind.LambdaExpression);
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			
			if (context.Symbol is IMethodSymbol methodSymbol)
			{
				EventType eventType = methodSymbol.GetEventHandlerType(pxContext);

				if (eventType != EventType.None)
				{
					foreach (var innerAnalyzer in _innerAnalyzers)
					{
						context.CancellationToken.ThrowIfCancellationRequested();
						innerAnalyzer.Analyze(context, pxContext, codeAnalysisSettings, eventType);
					}
				}
			}
		}

		private void AnalyzeLambda(OperationAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings)
		{
			if (context.Operation is ILambdaExpression lambdaExpression)
			{
				AnalyzeMethod(new SymbolAnalysisContext(
					lambdaExpression.Signature, 
					context.Compilation,
					context.Options, 
					context.ReportDiagnostic, 
					d => true, // this check is covered inside context.ReportDiagnostic
					context.CancellationToken),
					pxContext,
					codeAnalysisSettings);
			}
		}

		private CodeAnalysisSettings GetCodeAnalysisSettings()
		{
			if (_codeAnalysisSettings != null)
				return _codeAnalysisSettings; // for unit tests

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
