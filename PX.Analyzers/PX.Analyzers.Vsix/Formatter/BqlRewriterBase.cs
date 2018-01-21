using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PX.Analyzers.Vsix.Formatter
{
	abstract class BqlRewriterBase : CSharpSyntaxRewriter
	{
		protected SyntaxTrivia EndOfLineTrivia { get; }
		protected SyntaxTriviaList IndentationTrivia { get; }
		protected SyntaxTriviaList DefaultLeadingTrivia { get; }

		protected SemanticModel SemanticModel { get; }
		protected BqlContext Context { get; }

		protected BqlRewriterBase(BqlContext context, SemanticModel semanticModel,
			SyntaxTrivia endOfLineTrivia, SyntaxTriviaList indentationTrivia, SyntaxTriviaList defaultLeadingTrivia)
		{
			Context = context;
			SemanticModel = semanticModel;
			EndOfLineTrivia = endOfLineTrivia;
			IndentationTrivia = indentationTrivia;
			DefaultLeadingTrivia = defaultLeadingTrivia;
		}

		protected BqlRewriterBase(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia)
			: this(parent.Context, parent.SemanticModel, 
				  parent.EndOfLineTrivia, parent.IndentationTrivia, defaultLeadingTrivia)
		{
		}

		protected INamedTypeSymbol GetTypeSymbol(SyntaxNode node)
		{
			return SemanticModel.GetTypeInfo(node).Type as INamedTypeSymbol;
		}
	}
}
