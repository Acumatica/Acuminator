using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlSelectRewritingPlanner : BqlRewritingPlannerBase
	{
		private bool _tokenCompleted;
		private bool _identifierCompleted;
		
		public BqlSelectRewritingPlanner(BqlRewritingPlannerBase parent, SyntaxTriviaList defaultLeadingTrivia)
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override void VisitToken(SyntaxToken token)
		{
			if (!_tokenCompleted && token.IsKind(SyntaxKind.LessThanToken))
			{
				token = token.WithTrailingTrivia(EndOfLineTrivia);
				_tokenCompleted = true;
			}

			base.VisitToken(token);
		}

		public override void VisitIdentifierName(IdentifierNameSyntax node)
		{
			if (!_identifierCompleted)
			{
				var typeSymbol = GetTypeSymbol(node);
				if (typeSymbol != null && typeSymbol.ImplementsInterface(Context.IBqlTable))
				{
					node = node.WithLeadingTrivia(DefaultLeadingTrivia.AddRange(IndentationTrivia));
					_identifierCompleted = true;
				}
			}

			base.VisitIdentifierName(node);
		}
	}
}
