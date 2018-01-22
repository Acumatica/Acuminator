using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PX.Analyzers.Vsix.Formatter
{
	class MapSyntaxRewriter : CSharpSyntaxRewriter
	{
		private readonly IReadOnlyDictionary<SyntaxNode, SyntaxNode> _map;

		public MapSyntaxRewriter(IReadOnlyDictionary<SyntaxNode, SyntaxNode> map)
		{
			_map = map;
		}

		public override SyntaxNode Visit(SyntaxNode node)
		{
			if (node != null && _map.TryGetValue(node, out var newNode))
				node = newNode;

			return base.Visit(node);
		}
	}
}
