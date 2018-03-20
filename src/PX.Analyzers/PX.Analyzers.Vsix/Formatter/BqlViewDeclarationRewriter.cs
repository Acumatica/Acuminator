using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlViewDeclarationRewriter : BqlRewriterBase
	{
		public BqlViewDeclarationRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
		{
			return OnNewLineAndIndented(node);
		}
	}
}