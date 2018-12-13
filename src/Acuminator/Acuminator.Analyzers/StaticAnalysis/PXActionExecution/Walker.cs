using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Analyzers.StaticAnalysis.PXActionExecution
{
    internal class Walker : NestedInvocationWalker
	{
		private readonly SymbolAnalysisContext _context;
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
			methodSymbol = methodSymbol?.OriginalDefinition?.OverriddenMethod ?? methodSymbol?.OriginalDefinition;

			if (methodSymbol != null && _pxContext.PXAction.Press.Contains(methodSymbol))
			{
				ReportDiagnostic(PXDiagnosticAnalyzer.ReportDiagnosticWithSuppressionCheck,
					_context.ReportDiagnostic, _diagnosticDescriptor, node, _messageArgs);
			}
			else
			{
				base.VisitInvocationExpression(node);
			}
		}
	}
}
