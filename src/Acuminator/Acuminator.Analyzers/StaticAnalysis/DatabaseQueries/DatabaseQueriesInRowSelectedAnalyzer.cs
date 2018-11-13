using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
    public class DatabaseQueriesInRowSelectedAnalyzer : EventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1049_DatabaseQueriesInRowSelected);

		public override bool ShouldAnalyze(PXContext pxContext, CodeAnalysisSettings settings, EventType eventType) =>
			settings.IsvSpecificAnalyzersEnabled && eventType == EventType.RowSelected;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings,
									 EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			methodSyntax?.Accept(new Walker(context, pxContext, Descriptors.PX1049_DatabaseQueriesInRowSelected));
		}
	}
}
