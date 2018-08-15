using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Acuminator.Utilities;
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

			Document document = CreateDocument(source);
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			var syntaxRoot = document.GetSyntaxRootAsync().Result;
			//var namespaceSyntaxTree = syntaxRoot.DescendantNodes()
			//						   .OfType<NamespaceDeclarationSyntax>();

			List<bool> expected =	new List<bool>{false, true, false, false, true, false};
			List<bool> actual	=	new List<bool>();
			var pxContext = new PXContext(semanticModel.Compilation);

			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();
				foreach (var attribute in attributes)
				{
					var ai = new AttributeInformation(pxContext,semanticModel);
					var defaultAttribute = semanticModel.Compilation.GetTypeByMetadataName("PX.Data.PXDefaultAttribute");
					actual.Add(ai.AttributeDerivedFromClass(attribute.AttributeClass,defaultAttribute));
				}
			}
			
			Assert.Equal(expected, actual);
		}
	}
}
