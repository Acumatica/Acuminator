using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PX.Analyzers.Vsix.Formatter
{
	public static class BqlFormatter
	{
		class HelloWorldRewriter : CSharpSyntaxRewriter
		{
			public override SyntaxNode Visit(SyntaxNode node)
			{
				var result = base.Visit(node);
				if (result == null) return null;

				var leadingTrivia = result.GetLeadingTrivia();
				leadingTrivia = leadingTrivia.Add(SyntaxFactory.Tab);
				return result.WithLeadingTrivia(leadingTrivia);
			}
		}

		public static SyntaxNode Format(SyntaxNode syntaxRoot, SemanticModel semanticModel)
		{
			var rewriter = new HelloWorldRewriter();
			return rewriter.Visit(syntaxRoot);
		}
	}
}
