using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationInEventHandlers
{
    public class LongOperationInEventHandlersAnalyzer : IEventHandlerAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1046_LongOperationInEventHandlers);

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new StartLongOperationWalker(context, pxContext, Descriptors.PX1046_LongOperationInEventHandlers);

			methodSyntax?.Accept(walker);
		}
	}
}
