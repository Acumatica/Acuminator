using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace PX.Analyzers.Vsix.Formatter
{
	public class BqlFormatter
	{
		private readonly string _endOfLineCharacter;
		private readonly bool _useTabs;
		private readonly int _tabSize;
		private readonly int _indentSize;

		class HelloWorldRewriter : CSharpSyntaxRewriter
		{
			private readonly SyntaxTrivia EndOfLineTrivia;
			private readonly SyntaxTriviaList IndentationTrivia;

			public HelloWorldRewriter(BqlFormatter parent)
			{
				if (parent == null) throw new ArgumentNullException(nameof (parent));

				EndOfLineTrivia = SyntaxFactory.ElasticEndOfLine(parent._endOfLineCharacter);
				if (parent._useTabs || parent._indentSize >= parent._tabSize)
				{
					var items = Enumerable
						.Repeat(SyntaxFactory.ElasticTab, parent._indentSize / parent._tabSize)
						.Append(SyntaxFactory.ElasticWhitespace(new string(' ', parent._indentSize % parent._tabSize)));
					IndentationTrivia = SyntaxTriviaList.Empty.AddRange(items);
				}
				else
				{
					IndentationTrivia = SyntaxTriviaList.Create(SyntaxFactory.ElasticWhitespace(new string(' ', parent._indentSize)));
				}
			}

			public override SyntaxNode Visit(SyntaxNode node)
			{
				var result = base.Visit(node);
				if (result == null) return null;

				SyntaxTriviaList leadingTrivia = result.GetLeadingTrivia();
				leadingTrivia = leadingTrivia.AddRange(IndentationTrivia);
				return result.WithLeadingTrivia(leadingTrivia);
			}
		}

		public BqlFormatter(string endOfLineCharacter, bool useTabs, int tabSize, int indentSize)
		{
			_endOfLineCharacter = endOfLineCharacter;
			_useTabs = useTabs;
			_tabSize = tabSize;
			_indentSize = indentSize;
		}

		public SyntaxNode Format(SyntaxNode syntaxRoot, SemanticModel semanticModel)
		{
			var rewriter = new HelloWorldRewriter(this);
			return rewriter.Visit(syntaxRoot);
		}
	}
}
