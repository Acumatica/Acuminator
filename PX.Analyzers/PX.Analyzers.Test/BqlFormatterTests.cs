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
		private readonly BqlFormatter _formatter = new BqlFormatter("\r\n", true, 4, 4);

		[Theory]
		[EmbeddedFileData("BQL_raw.cs", "BQL_formatted.cs")]
		[EmbeddedFileData("BQL_static_raw.cs", "BQL_static_formatted.cs")]
		[EmbeddedFileData("BQL_attribute_raw.cs", "BQL_attribute_formatted.cs")]
		public void TestHelloWorld(string text, string expected)
		{
			Document document = CreateDocument(text);
			SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			SyntaxNode formattedNode = _formatter.Format(syntaxRoot, semanticModel);

			formattedNode = formattedNode.WithAdditionalAnnotations(Formatter.Annotation);
			string actual = formattedNode.ToFullString();

			actual.Should().Be(expected);
		}
	}
}
