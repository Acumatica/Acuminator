using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace PX.Analyzers.Vsix.Formatter
{
	class SelectedNodesCollector : CSharpSyntaxWalker
	{
		private readonly TextSpan _selectedSpan;
		private readonly List<SyntaxNode> _nodesWithinSpan = new List<SyntaxNode>();

		public IReadOnlyCollection<SyntaxNode> NodesWithinSpan => _nodesWithinSpan;

		public SelectedNodesCollector(TextSpan selectedSpan)
		{
			_selectedSpan = selectedSpan;
		}

		public override void Visit(SyntaxNode node)
		{
			if (node != null && node.SpanStart >= _selectedSpan.Start && node.Span.End <= _selectedSpan.End)
			{
				_nodesWithinSpan.Add(node);
			}
			else
			{
				base.Visit(node);
			}
		}
	}
}