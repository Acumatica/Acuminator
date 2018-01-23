using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Vsix.Formatter
{
	/// <summary>
	/// Collects all modifications without modifying original syntax tree
	/// </summary>
	class BqlRewriter : BqlRewriterBase
	{
		public BqlRewriter(BqlContext context, SemanticModel semanticModel,
			SyntaxTriviaList endOfLineTrivia, SyntaxTriviaList indentationTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia, SyntaxTriviaList.Empty)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			INamedTypeSymbol constructedFromSymbol = typeSymbol?.ConstructedFrom; // get generic type
			
			if (constructedFromSymbol != null 
				&& (constructedFromSymbol.InheritsFromOrEquals(Context.PXSelectBase) 
					|| constructedFromSymbol.ImplementsInterface(Context.IBqlCreator)))
			{
				var statementRewriter = new BqlStatementRewriter(this, GetDefaultLeadingTrivia(node));
				return statementRewriter.Visit(node);
			}
			
			return base.VisitGenericName(node);
		}

		public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
		{
			var defaultTrivia = GetDefaultLeadingTrivia(node);
			var newNode = (VariableDeclarationSyntax) base.VisitVariableDeclaration(node);

			if (newNode != node)
			{
				var childRewriter = new BqlViewDeclarationRewriter(this, defaultTrivia);
				newNode = (VariableDeclarationSyntax) childRewriter.Visit(newNode);
			}

			return newNode;
		}

		private SyntaxTriviaList GetDefaultLeadingTrivia(SyntaxNode node)
		{
			if (node == null) return SyntaxTriviaList.Empty;

			SyntaxToken firstToken = node.GetFirstToken();
			SyntaxToken token = firstToken;
			do
			{
				int leadingEol = token.LeadingTrivia.IndexOf(SyntaxKind.EndOfLineTrivia);
				if (leadingEol >= 0)
				{
					var triviaAfterEol = token.LeadingTrivia.Skip(leadingEol + 1).ToSyntaxTriviaList();
					return GetWhitespaceTrivia(triviaAfterEol);
				}

				int trailingEol = token.TrailingTrivia.IndexOf(SyntaxKind.EndOfLineTrivia);
				if (token != firstToken && trailingEol >= 0)
				{
					var trivia = token.TrailingTrivia.Skip(trailingEol + 1).ToList();
					// Concat trailing trivia from current token (starting from EOL) with leading trivia from previous token
					SyntaxToken nextToken = token.GetNextToken(true, true);
					if (nextToken.HasLeadingTrivia)
					{
						trivia.AddRange(nextToken.LeadingTrivia);
					}

					return GetWhitespaceTrivia(trivia.ToSyntaxTriviaList());
				}

				token = token.GetPreviousToken(true, true);
			} while (!token.IsKind(SyntaxKind.None));

			return SyntaxTriviaList.Empty;
		}

		private SyntaxTriviaList GetWhitespaceTrivia(SyntaxTriviaList input)
		{
			if (input.All(t => t.IsKind(SyntaxKind.WhitespaceTrivia)))
				return input;

			// If there are some unexpected trivias (non-whitespace), calculate approximate indent
			int totalLength = input.FullSpan.Length;
			int indentLength = IndentationTrivia.FullSpan.Length;
			var trivia = new List<SyntaxTrivia>();
			int repeatCount = totalLength / indentLength;
			if (totalLength % indentLength > 0 || indentLength % totalLength > 0)
				repeatCount++;
			for (int i = 0; i < repeatCount; i++)
			{
				trivia.AddRange(IndentationTrivia);
			}
			return trivia.ToSyntaxTriviaList();
		}
	}
}
