using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlAggregateRewriter : BqlRewriterBase
	{
		public BqlAggregateRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			INamedTypeSymbol originalSymbol = typeSymbol?.OriginalDefinition; // get generic type

			if (originalSymbol != null)
			{
				if (originalSymbol.InheritsFromOrEqualsGeneric(Context.GroupByBase))
				{
					return RewriteGenericNode(node, new BqlAggregateRewriter(this, IndentedDefaultTrivia));
				}

				if (originalSymbol.ImplementsInterface(Context.IBqlFunction))
				{
					return RewriteGenericNode(node, new BqlAggregateRewriter(this, DefaultLeadingTrivia));
				}
			}

			return base.VisitGenericName(node);
		}
	}
}