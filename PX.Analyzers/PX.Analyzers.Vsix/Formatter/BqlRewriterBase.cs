using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PX.Analyzers.Vsix.Formatter
{
	abstract class BqlRewriterBase : CSharpSyntaxRewriter
	{
		protected SyntaxTrivia EndOfLineTrivia { get; }
		protected SyntaxTriviaList IndentationTrivia { get; }

		protected SemanticModel SemanticModel { get; }
		protected BqlContext Context { get; }

		protected BqlRewriterBase(BqlContext context, SemanticModel semanticModel,
			SyntaxTrivia endOfLineTrivia, SyntaxTriviaList indentationTrivia)
		{
			Context = context;
			SemanticModel = semanticModel;
			EndOfLineTrivia = endOfLineTrivia;
			IndentationTrivia = indentationTrivia;
		}

		protected BqlRewriterBase(BqlRewriterBase parent)
			: this(parent.Context, parent.SemanticModel, parent.EndOfLineTrivia, parent.IndentationTrivia)
		{
		}

		protected T AddTrivia<T>(T node, SyntaxTriviaList toLeadingTrivia, SyntaxTriviaList toTrailingTrivia)
			where T : SyntaxNode
		{
			if (node == null) return null;

			if (toLeadingTrivia.Any())
			{
				SyntaxTriviaList leadingTrivia = node.GetLeadingTrivia();
				leadingTrivia = leadingTrivia.AddRange(toLeadingTrivia);
				node = node.WithLeadingTrivia(leadingTrivia);
			}

			if (toTrailingTrivia.Any())
			{
				SyntaxTriviaList trailingTrivia = node.GetTrailingTrivia();
				trailingTrivia = trailingTrivia.AddRange(toTrailingTrivia);
				node = node.WithTrailingTrivia(trailingTrivia);
			}

			return node;
		}

		protected SyntaxToken AddTrivia(SyntaxToken token, SyntaxTriviaList toLeadingTrivia, SyntaxTriviaList toTrailingTrivia)
		{
			if (toLeadingTrivia.Any())
			{
				SyntaxTriviaList leadingTrivia = token.LeadingTrivia;
				leadingTrivia = leadingTrivia.AddRange(toLeadingTrivia);
				token = token.WithLeadingTrivia(leadingTrivia);
			}

			if (toTrailingTrivia.Any())
			{
				SyntaxTriviaList trailingTrivia = token.TrailingTrivia;
				trailingTrivia = trailingTrivia.AddRange(toTrailingTrivia);
				token = token.WithTrailingTrivia(trailingTrivia);
			}

			return token;
		}

		protected INamedTypeSymbol GetTypeSymbol(SyntaxNode node)
		{
			return SemanticModel.GetTypeInfo(node).Type as INamedTypeSymbol;
		}
	}
}
