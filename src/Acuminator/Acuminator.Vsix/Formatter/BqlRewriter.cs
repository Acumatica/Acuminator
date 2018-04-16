using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Analyzers.Utilities;

namespace Acuminator.Vsix.Formatter
{
	/// <summary>
	/// Collects all modifications without modifying original syntax tree
	/// </summary>
	class BqlRewriter : BqlRewriterBase
	{
		class WithDefaultTriviaFrom : IDisposable
		{
			private readonly BqlRewriter _parent;
			private readonly SyntaxTriviaList _previous;

			public WithDefaultTriviaFrom(BqlRewriter parent, SyntaxNode node)
			{
				if (node == null) throw new ArgumentNullException(nameof (node));
				_parent = parent ?? throw new ArgumentNullException(nameof (parent));
				_previous = _parent._defaultLeadingTrivia;
				_parent._defaultLeadingTrivia = parent.GetDefaultLeadingTrivia(node);
			}

			public void Dispose()
			{
				_parent._defaultLeadingTrivia = _previous;
			}
		}

		private SyntaxTriviaList _defaultLeadingTrivia;
		protected override SyntaxTriviaList DefaultLeadingTrivia => _defaultLeadingTrivia;

		public BqlRewriter(BqlContext context, SemanticModel semanticModel,
			SyntaxTriviaList endOfLineTrivia, SyntaxTriviaList indentationTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia, SyntaxTriviaList.Empty)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			if (node.TypeArgumentList.Arguments.Count <= 1)
				return base.VisitGenericName(node);

			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			INamedTypeSymbol originalSymbol = typeSymbol?.OriginalDefinition; // get generic type
			
			if (originalSymbol != null 
				&& (originalSymbol.InheritsFromOrEqualsGeneric(Context.PXSelectBase) 
					|| originalSymbol.ImplementsInterface(Context.IBqlCreator)))
			{
				// First case is for complete statement (view declaration, PXSelect, etc.), second is for partial BQL statement
				var defaultTrivia = DefaultLeadingTrivia.Any() ? DefaultLeadingTrivia : GetDefaultLeadingTrivia(node);
				var statementRewriter = new BqlStatementRewriter(this, defaultTrivia);
				return statementRewriter.Visit(node);
			}
			
			return base.VisitGenericName(node);
		}

		public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
		{
			using (new WithDefaultTriviaFrom(this, node))
			{
				var newNode = (FieldDeclarationSyntax) base.VisitFieldDeclaration(node);

				if (newNode != node)
				{
					// Using rewriter because there might be multiple declarators
					var childRewriter = new BqlViewDeclarationRewriter(this, DefaultLeadingTrivia);
					newNode = (FieldDeclarationSyntax) childRewriter.Visit(newNode);
				}

				return newNode;
			}
		}

		public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
		{
			using (new WithDefaultTriviaFrom(this, node))
			{
				var newNode = (MemberAccessExpressionSyntax) base.VisitMemberAccessExpression(node);

				if (newNode != node)
				{
					newNode = newNode.WithOperatorToken(OnNewLineAndIndented(newNode.OperatorToken));
				}

				return newNode;
			}
		}


		private SyntaxTriviaList GetDefaultLeadingTrivia(SyntaxNode node)
		{
			if (node == null) return SyntaxTriviaList.Empty;

			SyntaxToken firstToken = node.GetFirstToken();
			SyntaxToken token = firstToken;
			do
			{
				int leadingEol = token.LeadingTrivia.LastIndexOf(SyntaxKind.EndOfLineTrivia);
				if (leadingEol >= 0)
				{
					var triviaAfterEol = token.LeadingTrivia.Skip(leadingEol + 1).ToSyntaxTriviaList();
					return GetWhitespaceTrivia(triviaAfterEol);
				}

				int trailingEol = token.TrailingTrivia.LastIndexOf(SyntaxKind.EndOfLineTrivia);
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
