using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
	
	public class AttributeInformationTests : DiagnosticVerifier
	{

		public AttributeInformationTests() { }

		[Theory]
		[EmbeddedFileData(@"Dac\PX1030\Unit\AttributeInformationSimpleDacTest.cs")]
		public void TestAttributeInformation(string source)
		{
	//		var tree = CSharpSyntaxTree.ParseText(source);
	//		var syntaxRoot = (CompilationUnitSyntax)tree.GetRoot();

			Document document = CreateDocument(source);
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			var syntaxRoot = (CompilationUnitSyntax)semanticModel.SyntaxTree.GetRoot();
			var namespaceSyntaxTree = syntaxRoot.DescendantNodes()
									   .OfType<NamespaceDeclarationSyntax>().Where(a => a.);
			 

			foreach (var _class in namespaceSyntaxTree)
			{
				var variables = _class.DescendantNodes().OfType<ClassDeclarationSyntax>();
				
			}
			//Semantic model 
			Assert.Equal(source,"");
		}
	}
}
