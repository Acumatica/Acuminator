﻿#nullable enable

using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
    public class LongOperationInEventHandlersAnalyzer : EventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1046_LongOperationInEventHandlers);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new StartLongOperationWalker(context, pxContext, Descriptors.PX1046_LongOperationInEventHandlers);

			methodSyntax?.Accept(walker);
		}
	}
}
