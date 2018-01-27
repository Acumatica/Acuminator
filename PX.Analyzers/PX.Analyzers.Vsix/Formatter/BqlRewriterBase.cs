using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PX.Analyzers.Vsix.Formatter
{
	abstract class BqlRewriterBase : CSharpSyntaxRewriter
	{
		protected SyntaxTriviaList EndOfLineTrivia { get; }
		protected SyntaxTriviaList IndentationTrivia { get; }
		protected virtual SyntaxTriviaList DefaultLeadingTrivia { get; }
		protected SyntaxTriviaList IndentedDefaultTrivia => DefaultLeadingTrivia.AddRange(IndentationTrivia);

		protected BqlContext Context { get; }
		private readonly SemanticModel _semanticModel;
		
		protected BqlRewriterBase(BqlContext context, SemanticModel semanticModel,
			SyntaxTriviaList endOfLineTrivia, SyntaxTriviaList indentationTrivia, SyntaxTriviaList defaultLeadingTrivia)
		{
			Context = context;
			_semanticModel = semanticModel;
			EndOfLineTrivia = endOfLineTrivia;
			IndentationTrivia = indentationTrivia;
			DefaultLeadingTrivia = defaultLeadingTrivia;
		}

		protected BqlRewriterBase(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia)
			: this(parent.Context, parent._semanticModel,
				  parent.EndOfLineTrivia, parent.IndentationTrivia, defaultLeadingTrivia)
		{
		}
		
		protected INamedTypeSymbol GetTypeSymbol(SyntaxNode node)
		{
			TypeInfo typeInfo = _semanticModel.Compilation.ContainsSyntaxTree(node.SyntaxTree)
				? _semanticModel.GetTypeInfo(node)
				: _semanticModel.GetSpeculativeTypeInfo(_semanticModel.SyntaxTree.Length, node, SpeculativeBindingOption.BindAsExpression);

			return typeInfo.Type as INamedTypeSymbol;
		}

		protected INamedTypeSymbol GetOriginalTypeSymbol(SyntaxNode node)
		{
			return GetTypeSymbol(node)?.OriginalDefinition;
		}

		protected T OnNewLineAndIndented<T>(T node)
			where T : SyntaxNode
		{
			if (node == null) return null;

			SyntaxToken previousToken = node.GetFirstToken().GetPreviousToken();
			SyntaxTriviaList trivia = GetNewLineAndIndentedTrivia(previousToken);
			return node.WithLeadingTrivia(trivia);
		}

		/// <summary>
		/// Moves node to a new line, indents it and visit it using provided rewriter.
		/// </summary>
		protected SyntaxNode RewriteGenericNode(GenericNameSyntax node, BqlRewriterBase rewriter)
		{
			var newNode = OnNewLineAndIndented(node);
			return newNode.WithTypeArgumentList((TypeArgumentListSyntax)rewriter.Visit(newNode.TypeArgumentList));
		}

		protected SyntaxToken OnNewLineAndIndented(SyntaxToken token)
		{
			SyntaxToken previousToken = token.GetPreviousToken();
			SyntaxTriviaList trivia = GetNewLineAndIndentedTrivia(previousToken);
			return token.WithLeadingTrivia(trivia);
		}

		private SyntaxTriviaList GetNewLineAndIndentedTrivia(SyntaxToken previousToken)
		{
			// If previous token has trailing trivia, and it ends with EOL, do not append EOL again
			var trivia = new List<SyntaxTrivia>();

			if (!previousToken.HasTrailingTrivia
			    || !previousToken.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia))
			{
				trivia.AddRange(EndOfLineTrivia);
			}

			trivia.AddRange(DefaultLeadingTrivia);
			trivia.AddRange(IndentationTrivia);

			return trivia.ToSyntaxTriviaList();
		}
	}
}
