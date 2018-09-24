using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache
{
    internal class Walker : NestedInvocationWalker
	{
		private SymbolAnalysisContext _context;
		private readonly PXContext _pxContext;
		private readonly DiagnosticDescriptor _diagnosticDescriptor;
		private readonly object[] _messageArgs;

		public Walker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor diagnosticDescriptor,
			params object[] messageArgs)
			: base(context.Compilation, context.CancellationToken)
		{
			pxContext.ThrowOnNull(nameof (pxContext));
			diagnosticDescriptor.ThrowOnNull(nameof (diagnosticDescriptor));

			_context = context;
			_pxContext = pxContext;
			_diagnosticDescriptor = diagnosticDescriptor;
			_messageArgs = messageArgs;
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			var methodSymbol = GetSymbol<IMethodSymbol>(node);

			if (methodSymbol != null && IsMethodForbidden(methodSymbol))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _diagnosticDescriptor, node, _messageArgs);
			}
			else
			{
				base.VisitInvocationExpression(node);
			}
		}

		private bool IsMethodForbidden(IMethodSymbol symbol)
		{
			var methodSymbol = symbol.OriginalDefinition?.OverriddenMethod ?? symbol.OriginalDefinition;
			return methodSymbol != null &&
				   (_pxContext.PXCache.Insert.Any(i => methodSymbol.Equals(i)) ||
                   _pxContext.PXCache.Update.Any(u => methodSymbol.Equals(u)) ||
                   _pxContext.PXCache.Delete.Any(d => methodSymbol.Equals(d)) ||
                   _pxContext.PXSelectBaseGeneric.Insert.Any(i => methodSymbol.Equals(i)) ||
                   _pxContext.PXSelectBaseGeneric.Update.Any(u => methodSymbol.Equals(u)) ||
                   _pxContext.PXSelectBaseGeneric.Delete.Any(d => methodSymbol.Equals(d)));
		}
	}
}
