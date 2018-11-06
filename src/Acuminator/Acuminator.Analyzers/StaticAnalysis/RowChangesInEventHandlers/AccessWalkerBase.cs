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
		/// Searches for a member access (conditional and unconditional) on a syntax node that satisfies a predicate
		/// </summary>
		private abstract class AccessWalkerBase : CSharpSyntaxWalker
		{
			protected SemanticModel SemanticModel { get; }

			public bool Success { get; private set; }

			protected AccessWalkerBase(SemanticModel semanticModel)
			{
				semanticModel.ThrowOnNull(nameof (semanticModel));
				
				SemanticModel = semanticModel;
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
				if (Predicate(node.Expression))
					Success = true;
			}

			public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
			{
				if (Predicate(node.Expression))
					Success = true;
			}

			protected abstract bool Predicate(ExpressionSyntax node);
		}

	}
}
