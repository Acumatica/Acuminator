using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache
{
	internal class Walker : NestedInvocationWalker
	{
		private static readonly ISet<string> MethodNames = new HashSet<string>(StringComparer.Ordinal)
		{
			"Insert" ,
			"Update",
			"Delete",
		};

		private SymbolAnalysisContext _context;
		private readonly PXContext _pxContext;
		private readonly DiagnosticDescriptor _diagnosticDescriptor;

		public Walker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor diagnosticDescriptor)
			: base(context.Compilation, context.CancellationToken)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_context = context;
			_pxContext = pxContext;
			_diagnosticDescriptor = diagnosticDescriptor;
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();

			var methodSymbol = GetSymbol<IMethodSymbol>(node);

			if (methodSymbol != null && IsMethodForbidden(methodSymbol))
			{
				ReportDiagnostic(OriginalNode ?? node);
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

		private void ReportDiagnostic(SyntaxNode node)
		{
			_context.ReportDiagnostic(Diagnostic.Create(
				_diagnosticDescriptor,
				(OriginalNode ?? node).GetLocation()));
		}
	}
}
