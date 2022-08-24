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
		private readonly SymbolAnalysisContext _context;
		private readonly DiagnosticDescriptor _diagnosticDescriptor;
		private readonly object[] _messageArgs;

		public Walker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor diagnosticDescriptor,
					  params object[] messageArgs)
			: base(pxContext, context.CancellationToken)
		{
			diagnosticDescriptor.ThrowOnNull(nameof (diagnosticDescriptor));

			_context = context;
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
				   (PxContext.PXCache.Insert.Any(i => methodSymbol.Equals(i)) ||
                   PxContext.PXCache.Update.Any(u => methodSymbol.Equals(u)) ||
                   PxContext.PXCache.Delete.Any(d => methodSymbol.Equals(d)) ||
                   PxContext.PXSelectBaseGeneric.Insert.Any(i => methodSymbol.Equals(i)) ||
                   PxContext.PXSelectBaseGeneric.Update.Any(u => methodSymbol.Equals(u)) ||
                   PxContext.PXSelectBaseGeneric.Delete.Any(d => methodSymbol.Equals(d)));
		}
	}
}
