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
		[EmbeddedFileData(@"BQL\Raw\View.cs", @"BQL\Formatted\View.cs")]
		//[EmbeddedFileData(@"BQL\Raw\StaticCall.cs", @"BQL\Formatted\StaticCall.cs")]
		//[EmbeddedFileData(@"BQL\Raw\SearchInAttribute.cs", @"BQL\Formatted\SearchInAttribute.cs")]
		public void FormatDocument(string text, string expected)
		{
			Document document = CreateDocument(text);
			SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			SyntaxNode formattedNode = _formatter.Format(syntaxRoot, semanticModel);

			formattedNode = formattedNode.WithAdditionalAnnotations(Formatter.Annotation);
			string actual = formattedNode.ToFullString();

			Normalize(actual).Should().Be(Normalize(expected));
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
