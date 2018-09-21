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
using FluentAssertions;
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
		public Task TestAttributeSimpleInformation(string source) =>
			TestAttributeInformationAsync(source, new List<bool> { false, true, false, false, true, false });
		

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public Task TestAggregateAttributeAsync(string source) =>
			TestAttributeInformationAsync(source, new List<bool> { true, true });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public Task TestAggregateRegursiveAttributeAsync(string source) =>
			TestAttributeInformationAsync(source, new List<bool> { true, false });

		private async Task TestAttributeInformationAsync(string source, List<bool> expected)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
			var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);

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
					actual.Add(attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, defaultAttribute));
				}
			}

			Assert.Equal(expected, actual);
		}

		/*
		 * Tests IsBoundAttribute 
		 */
		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		public Task TestAreBoundAttributesAsync(string source) =>
			TestIsBoundAttributeAsync(source, 
											new List<BoundType>
											{
												BoundType.Unbound,
												BoundType.Unbound,
												BoundType.Unbound,
												BoundType.DbBound,
												BoundType.Unbound,
												BoundType.Unbound
											});

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public Task TestAreBoundAggregateAttributesAsync(string source) =>
			TestIsBoundAttributeAsync(source, new List<BoundType> { BoundType.DbBound, BoundType.Unbound });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public Task TestAreBoundAggregateRecursiveAttributeAsync(string source) =>
			TestIsBoundAttributeAsync(source, new List<BoundType> { BoundType.Unbound, BoundType.DbBound });

		private async Task TestIsBoundAttributeAsync(string source, List<BoundType> expected)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
			var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);

			List<BoundType> actual = new List<BoundType>();
			var pxContext = new PXContext(semanticModel.Compilation);
			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();
			var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);

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
		public Task FieldBoundAttributesWithDynamicIsDBFieldSetInConstructorAsync(string source) =>
			TestIsDBFieldPropertyAsync(source,
								   new List<BoundType> { BoundType.DbBound, BoundType.Unbound });

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldWithDefinedAttributes.cs")]
		public Task FieldBoundAttributesWithDynamicIsDBFieldSetInAttributeDefinitionAsync(string source) =>
		   TestIsDBFieldPropertyAsync(source,
								  new List<BoundType> { BoundType.Unknown, BoundType.Unknown, BoundType.Unknown, BoundType.Unknown });

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldWithoutDefinedAttributes.cs", internalCodeFileNames: new string[] { @"ExternalAttributes1.cs", @"ExternalAttributes2.cs" })]
		public Task FieldBoundAttributesWithDynamicIsDBFieldSetInExternalAttributeDefinitionAsync(string source, string externalAttribute1, string externalAttribute2) =>
		   TestIsDBFieldPropertyAsync(source,
								  new List<BoundType> { BoundType.Unknown, BoundType.Unknown },
								  new string[] { externalAttribute1, externalAttribute2 });

		private async Task TestIsDBFieldPropertyAsync(string source, List<BoundType> expected, string[] code = null)
		{
			Document document = CreateDocument(source, externalCode: code);
			SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
			var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);

			List<BoundType> actual = new List<BoundType>();
			var pxContext = new PXContext(semanticModel.Compilation);
			var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);

			IEnumerable<PropertyDeclarationSyntax> properties = syntaxRoot.DescendantNodes()
																		  .OfType<PropertyDeclarationSyntax>()
																		  .Where(a => !a.AttributeLists.IsNullOrEmpty());

			foreach (PropertyDeclarationSyntax property in properties)
			{
				IPropertySymbol propertySymbol =  semanticModel.GetDeclaredSymbol(property);
				
				actual.Add((propertySymbol != null)?
								attributeInformation.ContainsBoundAttributes(propertySymbol.GetAttributes()):
								BoundType.Unknown);
			}

			actual.Should().BeEquivalentTo(expected);
		}

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		private  Task TestListOfParentsSimpleAsync(string source)
		{
			return TestListOfParentsAsync(source,
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
		[EmbeddedFileData(@"NotAcumaticaAttributeDac.cs")]
		private void TestDacWithNonAcumaticaAttribute(string source)
		{
			TestListOfParents(source,
								new List<List<string>>
								{
									new List<string>{ "PX.Data.PXDBIntAttribute", },
									new List<string>{ "PX.Data.PXDBIntAttribute", }
								});
		}


		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private  Task TestListOfParentsAggregateAsync(string source)
		{
			return TestListOfParentsAsync(source,
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
		private Task TestListOfParentsAggregateRecursiveAsync(string source)
		{
			return TestListOfParentsAsync(source, new List<List<string>> {
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
		private Task TestListOfParentsSimpleExpandedAsync(string source)
		{
			return TestListOfParentsAsync(source,
								new List<List<string>> {
									new List<string>{ "PX.Data.PXBoolAttribute" },
									new List<string>{ "PX.Data.PXDefaultAttribute" },
									new List<string>{ "PX.Data.PXUIFieldAttribute" },
									new List<string>{ "PX.Data.PXDBCalcedAttribute" },
									new List<string>{ "PX.Data.PXDefaultAttribute" },
									new List<string>{ "PX.Data.PXUIFieldAttribute" }
								},
								true);
		}


		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private Task TestListOfParentsAggregateExpandedAsync(string source)
		{
			return TestListOfParentsAsync(source,
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
								expand: true);

		}

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		private Task TestListOfParentsAggregateRecursiveExpandedAsync(string source)
		{
			return TestListOfParentsAsync(source, new List<List<string>> {
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
								expand: true);
		}


		private async Task TestListOfParentsAsync(string source, List<List<string>> expected, bool expand = false)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
			var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
			var pxContext = new PXContext(semanticModel.Compilation);

			var expectedSymbols = ConvertStringsToITypeSymbols(expected, semanticModel);

			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			List<HashSet<ITypeSymbol>> result = new List<HashSet<ITypeSymbol>>();

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);

				if (typeSymbol == null)
					continue;

				var attributes = typeSymbol.GetAttributes();

				foreach (var attribute in attributes)
				{
					var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);
					result.Add(attributeInformation.AttributesListDerivedFromClass(attribute.AttributeClass, expand).ToHashSet());
				}
			}

			Assert.Equal(expectedSymbols, result);
		}

		private List<HashSet<ITypeSymbol>> ConvertStringsToITypeSymbols(List<List<string>> expectedSymbolNamesSets, SemanticModel semanticModel)
		{
			var expectedSymbols = new List<HashSet<ITypeSymbol>>(capacity: expectedSymbolNamesSets.Count);

			foreach (List<string> symbolNamesSet in expectedSymbolNamesSets)
			{
				HashSet<ITypeSymbol> expectedSymbolsSet = new HashSet<ITypeSymbol>();

				foreach (string symbolName in symbolNamesSet)
				{
					expectedSymbolsSet.Add(semanticModel.Compilation.GetTypeByMetadataName(symbolName));
				}

				expectedSymbols.Add(expectedSymbolsSet);
			}

			return expectedSymbols;
		}
	}
}