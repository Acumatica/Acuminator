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
			SyntaxTrivia endOfLineTrivia, SyntaxTriviaList indentationTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia)
		{
		}

		public BqlSelectRewriter(BqlRewriterBase parent)
			: base(parent)
		{
		}

		public override SyntaxToken VisitToken(SyntaxToken token)
		{
			token = base.VisitToken(token);

			if (!_tokenCompleted && token.IsKind(SyntaxKind.LessThanToken))
			{
				token = AddTrivia(token, SyntaxTriviaList.Empty, SyntaxTriviaList.Create(EndOfLineTrivia));
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
					node = AddTrivia(node, IndentationTrivia, SyntaxTriviaList.Empty);
					_identifierCompleted = true;
				}
			}
			return node;
		}
	}
}
