using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	/// <summary>
	/// C# Syntax Walker that searches for the database queries
	/// </summary>
	internal class Walker : NestedInvocationWalker
	{
		private static readonly IEnumerable<string> MethodPrefixes = new[] { "Select", "Search", "Update", "Delete" };

		private readonly SymbolAnalysisContext _context;
		private readonly PXContext _pxContext;
		private readonly DiagnosticDescriptor _diagnosticDescriptor;

		public Walker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor diagnosticDescriptor)
			: base(context.Compilation, context.CancellationToken)
		{
			pxContext.ThrowOnNull(nameof(pxContext));
			diagnosticDescriptor.ThrowOnNull(nameof(diagnosticDescriptor));

			_context = context;
			_pxContext = pxContext;
			_diagnosticDescriptor = diagnosticDescriptor;
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax node)
		{
			ThrowIfCancellationRequested();
			
			var methodSymbol = GetSymbol<IMethodSymbol>(node);

			if (methodSymbol != null && IsDatabaseCall(methodSymbol))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _diagnosticDescriptor, node);
			}
			else
			{
				base.VisitInvocationExpression(node);
			}
		}

		private bool IsDatabaseCall(IMethodSymbol candidate)
		{
			var containingType = candidate.ContainingType?.OriginalDefinition;
			return MethodPrefixes.Any(p => candidate.Name.StartsWith(p, StringComparison.Ordinal))
				   && containingType != null
				   && (containingType.IsBqlCommand(_pxContext)
					   || containingType.InheritsFromOrEquals(_pxContext.PXViewType)
					   || containingType.InheritsFromOrEquals(_pxContext.PXSelectorAttribute)
					   || containingType.Equals(_pxContext.PXDatabase));
		}
	}
}
