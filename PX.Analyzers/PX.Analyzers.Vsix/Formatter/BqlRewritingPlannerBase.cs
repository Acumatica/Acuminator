using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PX.Analyzers.Vsix.Formatter
{
	abstract class BqlRewritingPlannerBase : CSharpSyntaxWalker
	{
		protected SyntaxTriviaList EndOfLineTrivia { get; }
		protected SyntaxTriviaList IndentationTrivia { get; }
		protected SyntaxTriviaList DefaultLeadingTrivia { get; }
		
		protected BqlContext Context { get; }
		protected SemanticModel SemanticModel { get; }

		public IReadOnlyDictionary<SyntaxNode, SyntaxTriviaList> Result => _result;
		private readonly Dictionary<SyntaxNode, SyntaxTriviaList> _result = new Dictionary<SyntaxNode, SyntaxTriviaList>();

		protected BqlRewritingPlannerBase(BqlContext context, SemanticModel semanticModel,
			SyntaxTriviaList endOfLineTrivia, SyntaxTriviaList indentationTrivia, SyntaxTriviaList defaultLeadingTrivia)
		{
			Context = context;
			SemanticModel = semanticModel;
			EndOfLineTrivia = endOfLineTrivia;
			IndentationTrivia = indentationTrivia;
			DefaultLeadingTrivia = defaultLeadingTrivia;
		}

		protected BqlRewritingPlannerBase(BqlRewritingPlannerBase parent, SyntaxTriviaList defaultLeadingTrivia)
			: this(parent.Context, parent.SemanticModel,
				  parent.EndOfLineTrivia, parent.IndentationTrivia, defaultLeadingTrivia)
		{
		}

		protected INamedTypeSymbol GetTypeSymbol(SyntaxNode node)
		{
			return SemanticModel.GetTypeInfo(node).Type as INamedTypeSymbol;
		}

		protected void Set(SyntaxNode originalNode, SyntaxTriviaList newNode)
		{
			_result[originalNode] = newNode;
		}

		protected void MergeWith(IReadOnlyDictionary<SyntaxNode, SyntaxTriviaList> values)
		{
			_result.MergeWith(values);
		}
	}
}
