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
		private class VariableMemberAccessWalker : CSharpSyntaxWalker
		{
			private readonly ImmutableHashSet<ILocalSymbol> _variables;
			private readonly SemanticModel _semanticModel;

			public bool Success { get; private set; }

			public VariableMemberAccessWalker(ImmutableHashSet<ILocalSymbol> variables, SemanticModel semanticModel)
			{
				semanticModel.ThrowOnNull(nameof (semanticModel));

				_variables = variables;
				_semanticModel = semanticModel;
			}

			public void Reset()
			{
				Success = false;
			}

			public override void Visit(SyntaxNode node)
			{
				if (!Success)
					base.Visit(node);
			}

			public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
			{
				if (IsVariable(node.Expression))
					Success = true;
			}

			public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
			{
				if (IsVariable(node.Expression))
					Success = true;
			}

			private bool IsVariable(ExpressionSyntax node)
			{
				return node != null
				       && _semanticModel.GetSymbolInfo(node).Symbol is ILocalSymbol variable
				       && _variables.Contains(variable);
			}
		}

	}
}
