using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlSelectRewriter : BqlRewriterBase
	{
		private bool _tokenCompleted;
		private bool _identifierCompleted;

		public BqlSelectRewriter(BqlContext context, SemanticModel semanticModel,
			SyntaxTrivia endOfLineTrivia, SyntaxTriviaList indentationTrivia, SyntaxTriviaList defaultLeadingTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia, defaultLeadingTrivia)
		{
		}

		public BqlSelectRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia)
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			token = base.VisitToken(token);

			if (!_tokenCompleted && token.IsKind(SyntaxKind.LessThanToken))
			{
				token = token.WithTrailingTrivia(EndOfLineTrivia);
				_tokenCompleted = true;
			}

			return token;
		}

		public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
		{
			node = (IdentifierNameSyntax)base.VisitIdentifierName(node);
			if (!_identifierCompleted)
			{
				var typeSymbol = GetTypeSymbol(node);
				if (typeSymbol != null && typeSymbol.ImplementsInterface(Context.IBqlTable))
				{
					node = node.WithLeadingTrivia(DefaultLeadingTrivia.AddRange(IndentationTrivia));
					_identifierCompleted = true;
				}
			}
			return node;
		}
	}
}
