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
    public class DatabaseQueriesInRowSelectedAnalyzer : IEventHandlerAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1049_DatabaseQueriesInRowSelected);

		public virtual bool ShouldAnalyze(PXContext pxContext, CodeAnalysisSettings settings) => settings.IsvSpecificAnalyzersEnabled;

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings, 
			EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			
			if (eventType == EventType.RowSelected)
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
				methodSyntax?.Accept(new Walker(context, pxContext, Descriptors.PX1049_DatabaseQueriesInRowSelected));
			}
		}
	}
}
