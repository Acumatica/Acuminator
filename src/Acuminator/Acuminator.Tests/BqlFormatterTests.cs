using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Vsix.Formatter;
using Acuminator.Tests.Helpers;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
	public class BqlFormatterTests : DiagnosticVerifier
	{
		protected const string EndOfLine = "\r\n";

		protected readonly BqlFormatter _formatter;

		public BqlFormatterTests()
			: this(new BqlFormatter(EndOfLine, true, 4, 4))
		{
		}

		protected BqlFormatterTests(BqlFormatter formatter)
		{
			_formatter = formatter;
		}

		[Theory]
		[EmbeddedFileData(@"BQL\Raw\View.cs", @"BQL\Formatted\View.cs")]
		[EmbeddedFileData(@"BQL\Raw\StaticCall.cs", @"BQL\Formatted\StaticCall.cs")]
		[EmbeddedFileData(@"BQL\Raw\StaticCall_GroupBy.cs", @"BQL\Formatted\StaticCall_GroupBy.cs")]
		[EmbeddedFileData(@"BQL\Raw\SearchInAttribute.cs", @"BQL\Formatted\SearchInAttribute.cs")]
		[EmbeddedFileData(@"BQL\Raw\View_JoinWhere2.cs", @"BQL\Formatted\View_JoinWhere2.cs")]
		[EmbeddedFileData(@"BQL\Raw\View_MultipleJoins.cs", @"BQL\Formatted\View_MultipleJoins.cs")]
		[EmbeddedFileData(@"BQL\Raw\View_Complex.cs", @"BQL\Formatted\View_Complex.cs")]
		[EmbeddedFileData(@"BQL\Raw\View_NestedWhere.cs", @"BQL\Formatted\View_NestedWhere.cs")]
		[EmbeddedFileData(@"BQL\Raw\View_EmptyLines.cs", @"BQL\Formatted\View_EmptyLines.cs")]
		[EmbeddedFileData(@"BQL\Raw\Search_Join.cs", @"BQL\Formatted\Search_Join.cs")]
		public virtual void FormatDocument(string text, string expected)
		{
			string actual = Format(text);
			Normalize(actual).Should().Be(Normalize(expected));
		}

		[Theory]
		[EmbeddedFileData(@"BQL\Formatted\View.cs")]
		[EmbeddedFileData(@"BQL\Formatted\StaticCall.cs")]
		[EmbeddedFileData(@"BQL\Formatted\StaticCall_GroupBy.cs")]
		[EmbeddedFileData(@"BQL\Formatted\SearchInAttribute.cs")]
		[EmbeddedFileData(@"BQL\Formatted\View_JoinWhere2.cs")]
		[EmbeddedFileData(@"BQL\Formatted\View_MultipleJoins.cs")]
		[EmbeddedFileData(@"BQL\Formatted\View_Complex.cs")]
		[EmbeddedFileData(@"BQL\Formatted\View_NestedWhere.cs")]
		[EmbeddedFileData(@"BQL\Formatted\View_EmptyLines.cs")]
		[EmbeddedFileData(@"BQL\Formatted\Search_Join.cs")]
		public virtual void ShouldNotDoubleFormat(string expected)
		{
			string actual = Format(expected);
			Normalize(actual).Should().Be(Normalize(expected));
		}

		[Theory]
		[EmbeddedFileDataWithParams(@"BQL\Raw\Search_Join.cs", @"BQL\Formatted\Search_Join.cs", 28, 28, 29, 35)]
		public virtual void FormatSelection(string text, string expected, 
			int startLine, int endLine,
			int expectedStartLine, int expectedEndLine)
		{
			Document document = CreateDocument(text);
			Document expectedDocument = CreateDocument(expected);
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;

			var originalNodes = GetSelectedNodes(document, startLine, endLine);
			var expectedNodes = GetSelectedNodes(expectedDocument, expectedStartLine, expectedEndLine);

			for (int i = 0; i < originalNodes.Count; i++)
			{
				SyntaxNode expectedNode = expectedNodes[i];
				SyntaxNode actualNode = _formatter.Format(originalNodes[i], semanticModel);
				Normalize(actualNode.ToFullString()).Should().Be(Normalize(expectedNode.ToFullString()));
			}
		}

		private IReadOnlyList<SyntaxNode> GetSelectedNodes(Document document, int startLine, int endLine)
		{
			SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			SourceText text = document.GetTextAsync().Result;
			
			int start = text.Lines[startLine - 1].Start;
			int end = text.Lines[endLine - 1].End;

			var selection = TextSpan.FromBounds(start, end);
			var walker = new SpanWalker(selection);
			walker.Visit(syntaxRoot.FindNode(selection));

			return walker.NodesWithinSpan.ToArray();
		}

		private string Format(string text)
		{
			Document document = CreateDocument(text);
			SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			SyntaxNode formattedNode = _formatter.Format(syntaxRoot, semanticModel);

			formattedNode = formattedNode.WithAdditionalAnnotations(Formatter.Annotation);
			string actual = formattedNode.ToFullString();

			return actual;
		}

		private string Normalize(string text)
		{
			return String.Join(EndOfLine, 
				text
				.Split(new[] { EndOfLine }, StringSplitOptions.None)
				.Select(line => line.TrimEnd()));
		}
	}

	public class BqlFormatterTestsUsingSpaces : BqlFormatterTests
	{
		protected const int IndentSize = 4;

		public BqlFormatterTestsUsingSpaces()
			: base(new BqlFormatter(EndOfLine, false, 4, IndentSize))
		{
		}

		public override void FormatDocument(string text, string expected)
		{
			text = ReplaceTabsWithSpaces(text);
			expected = ReplaceTabsWithSpaces(expected);

			base.FormatDocument(text, expected);
		}

		public override void FormatSelection(string text, string expected, int startLine, int endLine, int expectedStartLine,
			int expectedEndLine)
		{
			text = ReplaceTabsWithSpaces(text);
			expected = ReplaceTabsWithSpaces(expected);

			base.FormatSelection(text, expected, startLine, endLine, expectedStartLine, expectedEndLine);
		}

		public override void ShouldNotDoubleFormat(string expected)
		{
			expected = ReplaceTabsWithSpaces(expected);

			base.ShouldNotDoubleFormat(expected);
		}

		private string ReplaceTabsWithSpaces(string text)
		{
			if (String.IsNullOrEmpty(text)) return text;
			return text.Replace("\t", new string(' ', IndentSize));
		}
	}
}
