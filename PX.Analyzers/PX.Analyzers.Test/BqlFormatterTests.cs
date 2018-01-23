using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using PX.Analyzers.Test.Helpers;
using PX.Analyzers.Vsix.Formatter;
using TestHelper;
using Xunit;

namespace PX.Analyzers.Test
{
	public class BqlFormatterTests : DiagnosticVerifier
	{
		private const string EndOfLine = "\r\n";

		private readonly BqlFormatter _formatter = new BqlFormatter(EndOfLine, true, 4, 4);

		[Theory]
		[EmbeddedFileData(@"BQL\Common.cs", @"BQL\Raw\View.cs", @"BQL\Formatted\View.cs")]
		[EmbeddedFileData(@"BQL\Common.cs", @"BQL\Raw\StaticCall.cs", @"BQL\Formatted\StaticCall.cs")]
		[EmbeddedFileData(@"BQL\Common.cs", @"BQL\Raw\SearchInAttribute.cs", @"BQL\Formatted\SearchInAttribute.cs")]
		public void FormatDocument(string common, string text, string expected)
		{
			string actual = Format(common, text);
			Normalize(actual).Should().Be(Normalize(expected));
		}

		[Theory]
		[EmbeddedFileData(@"BQL\Common.cs", @"BQL\Formatted\View.cs")]
		[EmbeddedFileData(@"BQL\Common.cs", @"BQL\Formatted\StaticCall.cs")]
		[EmbeddedFileData(@"BQL\Common.cs", @"BQL\Formatted\SearchInAttribute.cs")]
		public void ShouldNotDoubleFormat(string common, string expected)
		{
			string actual = Format(common, expected);
			Normalize(actual).Should().Be(Normalize(expected));
		}

		private string Format(string common, string text)
		{
			Document document = CreateCSharpDocument(text, common);
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
}
