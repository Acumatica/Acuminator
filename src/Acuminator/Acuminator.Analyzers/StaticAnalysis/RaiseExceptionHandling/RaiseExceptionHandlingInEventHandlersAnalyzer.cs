#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.RaiseExceptionHandling
{
	public class RaiseExceptionHandlingInEventHandlersAnalyzer : EventHandlerAggregatedAnalyzerBase
	{
		private static readonly ISet<EventType> AnalyzedEventTypes = new HashSet<EventType>()
		{
			EventType.FieldDefaulting,
			EventType.FieldSelecting,
			EventType.RowSelecting,
			EventType.RowPersisted
		};

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers,
				Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonISV
			);

		public override bool ShouldAnalyze(PXContext pxContext, EventType eventType) =>
			AnalyzedEventTypes.Contains(eventType);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol) context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new Walker(context, pxContext, eventType);

			methodSyntax?.Accept(walker);
		}


		private class Walker : NestedInvocationWalker
		{
			private readonly SymbolAnalysisContext _context;
			private readonly EventType _eventType;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
				: base(pxContext, context.CancellationToken)
			{
				_context = context;
				_eventType= eventType;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				ThrowIfCancellationRequested();

				var methodSymbol = GetSymbol<IMethodSymbol>(node);
				methodSymbol = methodSymbol?.OriginalDefinition?.OverriddenMethod ?? methodSymbol?.OriginalDefinition;

				if (methodSymbol != null && PxContext.PXCache.RaiseExceptionHandling.Contains(methodSymbol))
				{
					if (!Settings.IsvSpecificAnalyzersEnabled && _eventType == EventType.FieldSelecting)
					{
						ReportDiagnostic(_context.ReportDiagnostic,
							Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonISV,
							node, _eventType);
					}
					else
					{
						ReportDiagnostic(_context.ReportDiagnostic,
							Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers,
							node, _eventType);
					}
				}
				else
				{
					base.VisitInvocationExpression(node);
				}
			}
		}
	}
}