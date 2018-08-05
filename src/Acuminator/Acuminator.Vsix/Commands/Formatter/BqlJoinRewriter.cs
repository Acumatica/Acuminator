using Acuminator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities;

namespace Acuminator.Vsix.Formatter
{
	class BqlJoinRewriter : BqlRewriterBase
	{
		public BqlJoinRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol originalSymbol = GetOriginalTypeSymbol(node);

			if (originalSymbol != null)
			{
				if (originalSymbol.ImplementsInterface(Context.IBqlJoin))
				{
					return RewriteGenericNode(node, new BqlJoinRewriter(this, DefaultLeadingTrivia));
				}

				if (originalSymbol.ImplementsInterface(Context.IBqlOn))
				{
					var rewriter = new BqlOnRewriter(this, IndentedDefaultTrivia);
					return rewriter.Visit(node);
				}
			}

			return base.VisitGenericName(node);
		}
	}

	class BqlOnRewriter : BqlRewriterBase
	{
		public BqlOnRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia)
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol originalSymbol = GetOriginalTypeSymbol(node);

			if (originalSymbol != null)
			{
				if (originalSymbol.ImplementsInterface(Context.IBqlOn))
				{
					return RewriteGenericNode(node, new BqlConditionRewriter(this, DefaultLeadingTrivia));
				}
			}

			return base.VisitGenericName(node);
		}
	}
}