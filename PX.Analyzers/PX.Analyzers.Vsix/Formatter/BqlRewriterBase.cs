using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PX.Analyzers.Vsix.Formatter
{
	abstract class BqlRewriterBase : CSharpSyntaxRewriter
	{
		protected SyntaxTriviaList EndOfLineTrivia { get; }
		protected SyntaxTriviaList IndentationTrivia { get; }
		protected SyntaxTriviaList DefaultLeadingTrivia { get; }
		
		protected BqlContext Context { get; }
		private SemanticModel _semanticModel;
		
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

		//public override SyntaxNode Visit(SyntaxNode node)
		//{
		//	if (node == null) return null;
		//	SyntaxNode newNode = base.Visit(node);
		//	if (newNode == null) return null;
			
		//	// update semantic model
		//	if (!_semanticModel.Compilation.ContainsSyntaxTree(newNode.SyntaxTree))
		//	{
		//		Compilation newCompilation = _semanticModel.Compilation
		//			.ReplaceSyntaxTree(node.SyntaxTree, newNode.SyntaxTree);
		//		_semanticModel = newCompilation.GetSemanticModel(_semanticModel.SyntaxTree);
		//	}

		//	return newNode;
		//}

		protected INamedTypeSymbol GetTypeSymbol(SyntaxNode node)
		{
			//if (!_semanticModel.Compilation.ContainsSyntaxTree(node.SyntaxTree))
			//{
			//	Compilation newCompilation = _semanticModel.Compilation
			//		.ReplaceSyntaxTree(_semanticModel.SyntaxTree, node.SyntaxTree);
			//	_semanticModel = newCompilation.GetSemanticModel(node.SyntaxTree);
			//}

			//return _semanticModel.GetTypeInfo(node).Type as INamedTypeSymbol;

			return _semanticModel
				.GetSpeculativeTypeInfo(_semanticModel.SyntaxTree.Length, node, SpeculativeBindingOption.BindAsExpression)
				.Type as INamedTypeSymbol;
		}
	}
}
