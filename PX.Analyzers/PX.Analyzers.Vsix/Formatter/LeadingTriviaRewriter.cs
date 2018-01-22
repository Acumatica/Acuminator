using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PX.Analyzers.Vsix.Formatter
{
	class LeadingTriviaRewriter : CSharpSyntaxRewriter
	{
		private readonly IReadOnlyDictionary<SyntaxNode, SyntaxTriviaList> _map;

		public LeadingTriviaRewriter(IReadOnlyDictionary<SyntaxNode, SyntaxTriviaList> map)
		{
			_map = map;
		}

		public override SyntaxNode Visit(SyntaxNode node)
		{
			var newNode = base.Visit(node);

			if (node != null && newNode != null && _map.TryGetValue(node, out var trivia))
			{
				newNode = newNode.WithLeadingTrivia(trivia);
			}

			return newNode;
		}
	}
}
