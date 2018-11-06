using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXActionExecution
{
	public class PXActionExecutionInEventHandlersAnalyzer : IEventHandlerAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1071_PXActionExecutionInEventHandlers);

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings, 
			EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol) context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new Walker(context, pxContext, Descriptors.PX1071_PXActionExecutionInEventHandlers);

			methodSyntax?.Accept(walker);
		}
	}
}
