using System.Collections.Immutable;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		/// <summary>
		/// Searches for a member access (conditional and unconditional) on a local variable
		/// </summary>
		private class VariableMemberAccessWalker : AccessWalkerBase
		{
			private readonly ImmutableHashSet<ILocalSymbol> _variables;
			
			public VariableMemberAccessWalker(ImmutableHashSet<ILocalSymbol> variables, SemanticModel semanticModel)
				:base(semanticModel)
			{
				_variables = variables;
			}
			
			protected override bool Predicate(ExpressionSyntax node)
			{
				return node != null
				       && SemanticModel.GetSymbolInfo(node).Symbol is ILocalSymbol variable
				       && _variables.Contains(variable);
			}
		}

	}
}
