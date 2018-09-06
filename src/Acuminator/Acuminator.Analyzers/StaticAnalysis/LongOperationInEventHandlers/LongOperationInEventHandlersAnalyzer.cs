using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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
			var walker = new Walker(context, pxContext);

			methodSyntax?.Accept(walker);
		}

		private class Walker : NestedInvocationWalker
		{
			private const string StartOperationMethodName = "StartOperation";

			private readonly SymbolAnalysisContext _context;
			private readonly PXContext _pxContext;

			public Walker(SymbolAnalysisContext context, PXContext pxContext)
				: base(context.Compilation, context.CancellationToken)
			{
				pxContext.ThrowOnNull(nameof (pxContext));

				_context = context;
				_pxContext = pxContext;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				ThrowIfCancellationRequested();

				var methodSymbol = GetSymbol<IMethodSymbol>(node);

				if (methodSymbol?.ContainingType?.OriginalDefinition != null
				    && methodSymbol.ContainingType.OriginalDefinition.Equals(_pxContext.PXLongOperation)
				    && methodSymbol.Name == StartOperationMethodName)
				{
					ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1046_LongOperationInEventHandlers, node);
				}
				else
				{
					base.VisitInvocationExpression(node);
				}
			}
		}
	}
}
