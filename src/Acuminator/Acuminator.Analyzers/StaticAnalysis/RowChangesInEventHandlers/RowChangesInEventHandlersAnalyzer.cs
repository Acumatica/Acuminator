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

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public class RowChangesInEventHandlersAnalyzer : IEventHandlerAnalyzer
	{
		private static readonly ISet<EventType> AnalyzedEventTypes = new HashSet<EventType>()
		{
			EventType.FieldDefaulting,
			EventType.FieldVerifying,
			EventType.RowSelected,
		};

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1047_RowChangesInEventHandlers);

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (AnalyzedEventTypes.Contains(eventType))
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;

				if (methodSyntax != null && (methodSyntax.Body != null || methodSyntax.ExpressionBody?.Expression != null))
				{
					var walker = new Walker(context, pxContext, eventType);
					methodSyntax.Accept(walker);
				}
			}
		}

		private class Walker : NestedInvocationWalker
		{
			private static readonly ISet<string> MethodNames = new HashSet<string>(StringComparer.Ordinal)
			{
				"SetValue" ,
				"SetValueExt",
				"SetDefaultExt",
			};

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

				if (methodSymbol != null && IsMethodForbidden(methodSymbol))
				{
					ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1047_RowChangesInEventHandlers, 
						node, _messageArgs);
				}
				else
				{
					base.VisitInvocationExpression(node);
				}
			}

			private bool IsMethodForbidden(IMethodSymbol symbol)
			{
				return symbol.ContainingType?.OriginalDefinition != null
				       && symbol.ContainingType.OriginalDefinition.InheritsFromOrEquals(_pxContext.PXCacheType)
				       && MethodNames.Contains(symbol.Name);
			}
		}

	}
}
