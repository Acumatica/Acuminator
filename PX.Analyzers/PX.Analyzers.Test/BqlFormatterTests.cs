using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		[EmbeddedFileData("BQL_raw.cs")]
		public void TestHelloWorld(string text)
		{
			Document document = CreateDocument(text);
			SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			SyntaxNode formattedNode = _formatter.Format(syntaxRoot, semanticModel);

			formattedNode = formattedNode.WithAdditionalAnnotations(Formatter.Annotation);
			string actual = formattedNode.ToFullString();
		}
	}
}
