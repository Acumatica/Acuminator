using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	/// <summary>
	/// C# Syntax Walker that searches for the database queries
	/// </summary>
	internal class Walker : NestedInvocationWalker
	{
		private const string SelectMethodName = "Select";
		private const string SearchMethodName = "Search";

		private readonly SymbolAnalysisContext _context;
		private readonly DiagnosticDescriptor _diagnosticDescriptor;
		private readonly ImmutableHashSet<IMethodSymbol> _databaseQueryMethods;

		public Walker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor diagnosticDescriptor)
			: base(pxContext, context.CancellationToken)
		{
			diagnosticDescriptor.ThrowOnNull(nameof(diagnosticDescriptor));

			_context = context;
			_diagnosticDescriptor = diagnosticDescriptor;

			_databaseQueryMethods = PxContext.PXDatabase.Select
				.Concat(PxContext.PXDatabase.Insert)
				.Concat(PxContext.PXDatabase.Update)
				.Concat(PxContext.PXDatabase.Delete)
				.Concat(PxContext.PXDatabase.Ensure)
				.Concat(PxContext.AttributeTypes.PXSelectorAttribute.Select)
				.Concat(PxContext.AttributeTypes.PXSelectorAttribute.GetItem)
				.Concat(PxContext.PXView.Select)
				.ToImmutableHashSet();
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
			var methodSymbol = candidate.OriginalDefinition?.OverriddenMethod ?? candidate.OriginalDefinition;

			if (methodSymbol != null && _databaseQueryMethods.Contains(methodSymbol))
				return true;

			// Check BQL Select / Search methods by name because
			// variations of these methods are declared in different PXSelectBase-derived classes
			var declaringType = candidate.ContainingType?.OriginalDefinition;

			if (declaringType != null && declaringType.IsBqlCommand(PxContext) &&
			    (candidate.Name.StartsWith(SelectMethodName, StringComparison.Ordinal) ||
			     candidate.Name.StartsWith(SearchMethodName, StringComparison.Ordinal)))
			{
				return true;
			}

			return false;
		}
	}
}
