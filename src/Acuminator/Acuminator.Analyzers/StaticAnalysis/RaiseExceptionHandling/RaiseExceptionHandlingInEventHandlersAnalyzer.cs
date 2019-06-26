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
using System.Collections.Generic;
using System.Collections.Immutable;

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
				Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonIsv
			);

		public override bool ShouldAnalyze(PXContext pxContext, EventType eventType) => 
			AnalyzedEventTypes.Contains(eventType);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new Walker(context, pxContext, eventType);

			methodSyntax?.Accept(walker);
		}

		
		private class Walker : NestedInvocationWalker
		{
			private readonly SymbolAnalysisContext _context;
			private readonly PXContext _pxContext;
			private readonly object[] _messageArgs;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, params object[] messageArgs)
				: base(context.Compilation, context.CancellationToken, pxContext.CodeAnalysisSettings)
			{
				pxContext.ThrowOnNull(nameof (pxContext));

				_context = context;
				_pxContext = pxContext;
				_messageArgs = messageArgs;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				ThrowIfCancellationRequested();

				var methodSymbol = GetSymbol<IMethodSymbol>(node);
				methodSymbol = methodSymbol?.OriginalDefinition?.OverriddenMethod ?? methodSymbol?.OriginalDefinition;

				if (methodSymbol != null && _pxContext.PXCache.RaiseExceptionHandling.Contains(methodSymbol))
				{
					if (!_pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled &&
					    _messageArgs.Contains(EventType.FieldSelecting))
					{
						ReportDiagnostic(_context.ReportDiagnostic,
							Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonIsv,
							node, _messageArgs);
					}else
					{
						ReportDiagnostic(_context.ReportDiagnostic,
							Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers, 
							node, _messageArgs);
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
