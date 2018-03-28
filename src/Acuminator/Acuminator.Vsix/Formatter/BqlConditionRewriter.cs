using Acuminator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Analyzers.Utilities;

namespace Acuminator.Vsix.Formatter
{
	class BqlConditionRewriter : BqlRewriterBase
	{
		private bool _firstWhereInParenthesis;
		private bool _externalMode;

		public BqlConditionRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
			_externalMode = true;
		}

		private BqlConditionRewriter(BqlConditionRewriter parent, SyntaxTriviaList defaultLeadingTrivia)
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			bool moveWhereToNextLine = _firstWhereInParenthesis || _externalMode;
			_firstWhereInParenthesis = false;

			INamedTypeSymbol originalSymbol = GetOriginalTypeSymbol(node);
			
			if (originalSymbol != null)
			{
				if (originalSymbol.InheritsFromOrEqualsGeneric(Context.Where2)
					|| originalSymbol.InheritsFromOrEqualsGeneric(Context.And2)
					|| originalSymbol.InheritsFromOrEqualsGeneric(Context.Or2))
				{
					var childRewriter = new BqlConditionRewriter(this, IndentedDefaultTrivia)
					{
						_firstWhereInParenthesis = true
					};
					return RewriteGenericNode(node, childRewriter);
				}
				
				if (originalSymbol.ImplementsInterface(Context.IBqlWhere)) // "parenthesis" in BQL statement
				{
					if (moveWhereToNextLine)
					{
						return RewriteGenericNode(node, new BqlConditionRewriter(this, IndentedDefaultTrivia));
					}

					// Leave Where on the same line, but indent and move all logic operators within the content to a new line
					var childRewriter = new BqlConditionRewriter(this, IndentedDefaultTrivia);
					return node.WithTypeArgumentList((TypeArgumentListSyntax) childRewriter.Visit(node.TypeArgumentList));
				}

				if (originalSymbol.ImplementsInterface(Context.IBqlPredicateChain))
				{
					return RewriteGenericNode(node, new BqlConditionRewriter(this, DefaultLeadingTrivia));
				}
			}

			return base.VisitGenericName(node);
		}
	}
}