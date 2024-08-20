
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache
{
	public class ChangesInPXCacheInEventHandlersAnalyzer : EventHandlerAggregatedAnalyzerBase
	{
		private static readonly ImmutableHashSet<EventType> AnalyzedEventTypes = new []
		{
			EventType.FieldDefaulting,
			EventType.FieldVerifying,
			EventType.RowSelected,
			EventType.RowSelecting,
		}.ToImmutableHashSet();

		private static readonly ImmutableHashSet<EventType> AnalyzedEventTypesForIsv = AnalyzedEventTypes
			.Add(EventType.RowInserting)
			.Add(EventType.RowUpdating)
			.Add(EventType.RowDeleting);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1044_ChangesInPXCacheInEventHandlers);

		public override bool ShouldAnalyze(PXContext pxContext, EventType eventType)
		{
			if (!base.ShouldAnalyze(pxContext, eventType))
				return false;

			var eventSet = pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled
				? AnalyzedEventTypesForIsv
				: AnalyzedEventTypes;

			return eventSet.Contains(eventType);
		}

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new Walker(context, pxContext, Descriptors.PX1044_ChangesInPXCacheInEventHandlers, eventType);

			methodSyntax?.Accept(walker);
		}
	}
}
