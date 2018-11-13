using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
	public class RaiseExceptionHandlingInEventHandlersAnalyzer : IEventHandlerAnalyzer
	{
		private static readonly ISet<EventType> AnalyzedEventTypes = new HashSet<EventType>()
		{
			EventType.FieldDefaulting,
			EventType.FieldSelecting,
			EventType.RowSelecting,
			EventType.RowPersisted
		};

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers);

		public virtual bool ShouldAnalyze(PXContext pxContext, CodeAnalysisSettings settings) => true;

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings, 
			EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (AnalyzedEventTypes.Contains(eventType))
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
				var walker = new Walker(context, pxContext, eventType);

				methodSyntax?.Accept(walker);
			}
		}

		
		private class Walker : NestedInvocationWalker
		{
			private readonly SymbolAnalysisContext _context;
			private readonly PXContext _pxContext;
			private readonly object[] _messageArgs;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, params object[] messageArgs)
				: base(context.Compilation, context.CancellationToken)
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
					ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers, 
						node, _messageArgs);
				}
				else
				{
					base.VisitInvocationExpression(node);
				}
			}
		}
	}
}
