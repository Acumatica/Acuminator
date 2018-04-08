using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Vsix.Formatter
{
	/// <summary>
	/// Finds nodes within provided span.
	/// </summary>
	public class SpanWalker : CSharpSyntaxWalker
	{
		private readonly TextSpan _span;
		private readonly List<SyntaxNode> _nodesWithinSpan = new List<SyntaxNode>();

		public IReadOnlyCollection<SyntaxNode> NodesWithinSpan => _nodesWithinSpan;

		public SpanWalker(TextSpan span)
		{
			_span = span;
		}

		public override void Visit(SyntaxNode node)
		{
			if (node != null && node.Span.Start >= _span.Start && node.Span.End <= _span.End)
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
