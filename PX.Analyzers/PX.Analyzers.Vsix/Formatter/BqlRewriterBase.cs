using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
			return _semanticModel
				.GetSpeculativeTypeInfo(_semanticModel.SyntaxTree.Length, node, SpeculativeBindingOption.BindAsExpression)
				.Type as INamedTypeSymbol;
		}

		protected T OnNewLineAndIndented<T>(T node)
			where T : SyntaxNode
		{
			return node?.WithLeadingTrivia(EndOfLineTrivia.AddRange(DefaultLeadingTrivia).AddRange(IndentationTrivia));
		}

		protected SyntaxToken OnNewLineAndIndented(SyntaxToken token)
		{
			return token.WithLeadingTrivia(EndOfLineTrivia.AddRange(DefaultLeadingTrivia).AddRange(IndentationTrivia));
		}
	}
}
