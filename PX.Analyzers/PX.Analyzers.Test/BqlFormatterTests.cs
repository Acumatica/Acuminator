using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using PX.Analyzers.Vsix.Formatter;
using TestHelper;
using Xunit;

namespace PX.Analyzers.Test
{
	public class BqlFormatterTests : DiagnosticVerifier
	{
		private readonly BqlFormatter _formatter = new BqlFormatter("\r\n", true, 4, 4);

		[Fact]
		public void TestHelloWorld()
		{
			string query = "PXSelect<SOOrder, \r\nWhere<SOOrder.orderType, \r\n\tEqual<SOOrder.quote>>";

			Document document = CreateDocument(query);
			SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			SyntaxNode formattedNode = _formatter.Format(syntaxRoot, semanticModel);

			string actual = formattedNode.ToFullString();
		}
	}
}
