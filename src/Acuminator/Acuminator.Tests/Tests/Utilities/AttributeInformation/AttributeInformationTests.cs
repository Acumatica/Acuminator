using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Tests.Helpers;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using static Acuminator.Tests.Verification.VerificationHelper;

using Acuminator.Utilities.Roslyn.PXFieldAttributes;

namespace Acuminator.Tests.Tests.Utilities.AttributeInformation
{

	public class AttributeInformationTests : Verification.DiagnosticVerifier
	{
		/* 
		*  Tests attribute derived 
		* */
		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		public async void TestAttributeSimpleInformation(string source) =>
			await TestAttributeInformationAsync(source, new List<bool> { false, true, false, false, true, false });

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public async void TestAggregateAttributeAsync(string source) =>
			await TestAttributeInformationAsync(source, new List<bool> { true, true });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public async void TestAggregateRegursiveAttributeAsync(string source) =>
			await TestAttributeInformationAsync(source, new List<bool> { true, false });

		private async Task TestAttributeInformationAsync(string source, List<bool> expected)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var syntaxRoot = document.GetSyntaxRootAsync().Result;

			List<bool> actual = new List<bool>();
			var pxContext = new PXContext(semanticModel.Compilation);

			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();
				foreach (var attribute in attributes)
				{
					var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);
					var defaultAttribute = pxContext.AttributeTypes.PXDefaultAttribute;
					actual.Add(attributeInformation.AttributeDerivedFromClass(attribute.AttributeClass, defaultAttribute));
				}
			}
			Assert.Equal(expected, actual);
		}

		/*
		 * Tests IsBoundAttribute 
		 */
		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		public async void TestAreBoundAttributesAsync(string source) =>
			await _testIsBoundAttributeAsync(source, new List<BoundAttribute> { BoundAttribute.Unbound,
														  BoundAttribute.Unbound,
														  BoundAttribute.Unbound,
														  BoundAttribute.DbBound,
														  BoundAttribute.Unbound,
														  BoundAttribute.Unbound });

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public async void TestAreBoundAggregateAttributesAsync(string source) =>
			await _testIsBoundAttributeAsync(source, new List<BoundAttribute> { BoundAttribute.DbBound, BoundAttribute.Unbound });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public async void TestAreBoundAggregateRecursiveAttributeAsync(string source) =>
			await _testIsBoundAttributeAsync(source, new List<BoundAttribute> { BoundAttribute.Unbound, BoundAttribute.DbBound });

		private async Task _testIsBoundAttributeAsync(string source, List<BoundAttribute> expected)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var syntaxRoot = document.GetSyntaxRootAsync().Result;

			List<BoundAttribute> actual = new List<BoundAttribute>();
			var pxContext = new PXContext(semanticModel.Compilation);

			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();
				foreach (var attribute in attributes)
				{
					actual.Add(attributeInformation.IsBoundAttribute(attribute));
				}
			}
			Assert.Equal(expected, actual);
		}

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldAttribute.cs")]
		public async void FieldBoundAttributesWithDynamicIsDBFieldSetInConstructorAsync(string source) =>
			await _testIsDBFieldPropertyAsync(source,
								   new List<BoundAttribute> { BoundAttribute.DbBound, BoundAttribute.Unbound });

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldWithDefinedAttributes.cs")]
		public async void FieldBoundAttributesWithDynamicIsDBFieldSetInAttributeDefinitionAsync(string source) =>
		   await _testIsDBFieldPropertyAsync(source,
								  new List<BoundAttribute> { BoundAttribute.Unknown, BoundAttribute.Unknown, BoundAttribute.Unknown, BoundAttribute.Unknown });

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldWithoutDefinedAttributes.cs", internalCodeFileNames: new string[] { @"ExternalAttributes1.cs", @"ExternalAttributes2.cs" })]
		public async void FieldBoundAttributesWithDynamicIsDBFieldSetInExternalAttributeDefinitionAsync(string source, string externalAttribute1, string externalAttribute2) =>
		   await _testIsDBFieldPropertyAsync(source,
								  new List<BoundAttribute> { BoundAttribute.Unknown, BoundAttribute.Unknown },
								  new string[] { externalAttribute1, externalAttribute2 });

		private async Task _testIsDBFieldPropertyAsync(string source, List<BoundAttribute> expected, string[] code = null)
		{
			Document document = CreateDocument(source, externalCode: code);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var syntaxRoot = document.GetSyntaxRootAsync().Result;

			List<BoundAttribute> actual = new List<BoundAttribute>();
			var pxContext = new PXContext(semanticModel.Compilation);
			var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);

			IEnumerable<PropertyDeclarationSyntax> properties = syntaxRoot.DescendantNodes()
																		  .OfType<PropertyDeclarationSyntax>()
																		  .Where(a => !a.AttributeLists.IsNullOrEmpty());

			foreach (PropertyDeclarationSyntax property in properties)
			{
				actual.Add(attributeInformation.ContainsBoundAttributes(semanticModel.GetDeclaredSymbol(property).GetAttributes()));
			}

			Assert.Equal(expected, actual);
		}

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		private async void TestListOfParentsSimpleAsync(string source)
		{
			await _testListOfParentsAsync(source,
								new List<List<string>> {
									new List<string>{ "PX.Data.PXBoolAttribute" },
									new List<string>{ "PX.Data.PXDefaultAttribute" },
									new List<string>{ "PX.Data.PXUIFieldAttribute" },
									new List<string>{ "PX.Data.PXDBCalcedAttribute" },
									new List<string>{ "PX.Data.PXDefaultAttribute"},
									new List<string>{ "PX.Data.PXUIFieldAttribute"}
								});
		}


		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private async void TestListOfParentsAggregateAsync(string source)
		{
			await _testListOfParentsAsync(source,
								new List<List<string>> {
									new List<string>
									{
										"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
										"PX.Objects.HackathonDemo.NonNullableIntAttribute",
										"PX.Data.PXDBIntAttribute",
										"PX.Data.PXDefaultAttribute",
										"PX.Data.PXIntListAttribute"
									},
									new List<string>
									{
										"PX.Objects.HackathonDemo.PXAccountAttribute",
										"PX.Objects.HackathonDemo.PXCustomDefaultAttribute"
									}
								});

		}

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		private async void TestListOfParentsAggregateRecursiveAsync(string source)
		{
			await _testListOfParentsAsync(source, new List<List<string>> {
									new List<string>
									{
										"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
										"PX.Objects.HackathonDemo.NonNullableIntAttribute",
										"PX.Data.PXDefaultAttribute",
										"PX.Data.PXIntListAttribute"
									},
									new List<string>
									{
										"PX.Objects.HackathonDemo._NonNullableIntListAttribute",
										"PX.Objects.HackathonDemo._NonNullableIntAttribute",
										"PX.Data.PXDBIntAttribute",
										"PX.Data.PXIntListAttribute"
									}
								});
		}

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		private async void TestListOfParentsSimpleExpandedAsync(string source)
		{
			await _testListOfParentsAsync(source,
								new List<List<string>> {
									new List<string>{ "PX.Data.PXBoolAttribute" },
									new List<string>{ "PX.Data.PXDefaultAttribute" },
									new List<string>{ "PX.Data.PXUIFieldAttribute" },
									new List<string>{ "PX.Data.PXDBCalcedAttribute"},
									new List<string>{ "PX.Data.PXDefaultAttribute"},
									new List<string>{ "PX.Data.PXUIFieldAttribute"}
								},
								true);
		}


		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private async void TestListOfParentsAggregateExpandedAsync(string source)
		{
			await _testListOfParentsAsync(source,
								new List<List<string>> {
									new List<string>
									{
										"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
										"PX.Data.PXAggregateAttribute",
										"PX.Objects.HackathonDemo.NonNullableIntAttribute",
										"PX.Data.PXDBIntAttribute",
										"PX.Data.PXDBFieldAttribute",
										"PX.Data.PXDefaultAttribute",
										"PX.Data.PXIntListAttribute"
									},
									new List<string>
									{
										"PX.Objects.HackathonDemo.PXAccountAttribute",
										"PX.Data.PXAggregateAttribute",
										"PX.Objects.HackathonDemo.PXCustomDefaultAttribute",
										"PX.Data.PXDefaultAttribute"
									}
								},
								true);

		}

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		private async void TestListOfParentsAggregateRecursiveExpandedAsync(string source)
		{
			await _testListOfParentsAsync(source, new List<List<string>> {
									new List<string>
									{
										"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
										"PX.Data.PXAggregateAttribute",
										"PX.Objects.HackathonDemo.NonNullableIntAttribute",
										"PX.Data.PXDefaultAttribute",
										"PX.Data.PXIntListAttribute"
									},
									new List<string>
									{
										"PX.Objects.HackathonDemo._NonNullableIntListAttribute",
										"PX.Data.PXAggregateAttribute",
										"PX.Objects.HackathonDemo._NonNullableIntAttribute",
										"PX.Data.PXDBIntAttribute",
										"PX.Data.PXDBFieldAttribute",
										"PX.Data.PXIntListAttribute"
									}
								},
								true);
		}


		private async Task _testListOfParentsAsync(string source, List<List<string>> expected, bool expand = false)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var syntaxRoot = document.GetSyntaxRootAsync().Result;

			var pxContext = new PXContext(semanticModel.Compilation);

			var expectedSymbols = _convertStringsToITypeSymbols(expected, semanticModel);

			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			List<HashSet<ITypeSymbol>> result = new List<HashSet<ITypeSymbol>>();

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();

				foreach (var attribute in attributes)
				{
					var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);
					result.Add(attributeInformation.AttributesListDerivedFromClass(attribute.AttributeClass, expand).ToHashSet());
				}
			}
			Assert.Equal(expectedSymbols, result);
		}

		private List<HashSet<ITypeSymbol>> _convertStringsToITypeSymbols(List<List<string>> expected, SemanticModel semanticModel)
		{
			var expectedSymbols = new List<HashSet<ITypeSymbol>>();
			foreach (var symbolsArray in expected)
			{
				HashSet<ITypeSymbol> attributesHashSet = new HashSet<ITypeSymbol>();
				foreach (var symbol in symbolsArray)
				{
					attributesHashSet.Add(semanticModel.Compilation.GetTypeByMetadataName(symbol));
				}
				expectedSymbols.Add(attributesHashSet);
			}
			return expectedSymbols;
		}

	}
}