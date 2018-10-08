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

namespace Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache
{
	public class ChangesInPXCacheInEventHandlersAnalyzer : IEventHandlerAnalyzer
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

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1044_ChangesInPXCacheInEventHandlers);

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings, 
			EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var eventSet = codeAnalysisSettings.IsvSpecificAnalyzersEnabled
				? AnalyzedEventTypesForIsv
				: AnalyzedEventTypes;

			if (eventSet.Contains(eventType))
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
				var walker = new Walker(context, pxContext, 
					Descriptors.PX1044_ChangesInPXCacheInEventHandlers, eventType);

				methodSyntax?.Accept(walker);
			}
		}
	}
}
