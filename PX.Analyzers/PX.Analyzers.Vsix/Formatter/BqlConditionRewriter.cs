using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlConditionRewriter : BqlRewriterBase
	{
		public BqlConditionRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol originalSymbol = GetOriginalTypeSymbol(node);

			if (originalSymbol != null && originalSymbol.ImplementsInterface(Context.IBqlPredicateChain))
			{
				SyntaxTriviaList trivia = DefaultLeadingTrivia;
				TypeSyntax firstArg = node.TypeArgumentList.Arguments.FirstOrDefault();
				if (firstArg != null)
				{
					INamedTypeSymbol firstArgOriginalSymbol = GetOriginalTypeSymbol(firstArg);
					if (firstArgOriginalSymbol != null && firstArgOriginalSymbol.ImplementsInterface(Context.IBqlWhere))
						trivia = IndentedDefaultTrivia;
				}

				return RewriteGenericNode(node, new BqlConditionRewriter(this, trivia));
			}

			return base.VisitGenericName(node);
		}
	}
}