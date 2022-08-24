using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic
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
			methodSymbol = methodSymbol?.OriginalDefinition?.OverriddenMethod ?? methodSymbol?.OriginalDefinition;

			if (methodSymbol != null && PxContext.UiPresentationLogicMethods.Contains(methodSymbol))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _diagnosticDescriptor, node, _messageArgs);
			}
			else
			{
				base.VisitInvocationExpression(node);
			}
		}
	}
}
