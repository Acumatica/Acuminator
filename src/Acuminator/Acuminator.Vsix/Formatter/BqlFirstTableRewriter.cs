using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.Formatter
{
	class BqlFirstTableRewriter : BqlRewriterBase
	{
		private bool _visited;

		public BqlFirstTableRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
		{
			if (!_visited)
			{
				_visited = true;
				return OnNewLineAndIndented(node);
			}

			return base.VisitIdentifierName(node);
		}
	}
}