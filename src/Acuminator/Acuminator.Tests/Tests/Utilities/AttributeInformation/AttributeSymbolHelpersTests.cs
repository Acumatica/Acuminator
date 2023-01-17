#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

using static Acuminator.Tests.Verification.VerificationHelper;

namespace Acuminator.Tests.Tests.Utilities.AttributeSymbolHelpersTests
{
	public class AttributeSymbolHelpersTests
	{
		/* 
		 *  Tests attribute derived 
		 * */
		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		public Task AttributesOnPropertiesAreDerivedFromPXDefault_SimpleAttributes(string source) =>
			CheckIfAttributesOnPropertiesAreDerivedFromPXDefaultAsync(source, new List<bool> { false, true, false, false, true, false });
		

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public Task AttributesOnPropertiesAreDerivedFromPXDefault_AggregateAttribute(string source) =>
			CheckIfAttributesOnPropertiesAreDerivedFromPXDefaultAsync(source, new List<bool> { true, true });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public Task AttributesOnPropertiesAreDerivedFromPXDefault_AggregateOnAggregateAttribute(string source) =>
			CheckIfAttributesOnPropertiesAreDerivedFromPXDefaultAsync(source, new List<bool> { true, false });

		private async Task CheckIfAttributesOnPropertiesAreDerivedFromPXDefaultAsync(string source, List<bool> expected)
		{
			Document document = CreateDocument(source);
			var (semanticModel, syntaxRoot) = await document.GetSemanticModelAndRootAsync().ConfigureAwait(false);
			semanticModel.ThrowOnNull(nameof(semanticModel));
			syntaxRoot.ThrowOnNull(nameof(syntaxRoot));

			List<bool> actual = new List<bool>(capacity: expected.Count);
			var pxContext = new PXContext(semanticModel.Compilation, CodeAnalysisSettings.Default);
			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();
			var pxdefaultAttribute = pxContext.AttributeTypes.PXDefaultAttribute;

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();

				foreach (var attribute in attributes)
				{
					actual.Add(attribute.AttributeClass.IsDerivedFromAttribute(pxdefaultAttribute, pxContext));
				}
			}

			Assert.Equal(expected, actual);
		}

		/*
		 * Tests IsBoundAttribute 
		 */
		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		public Task AreBoundAttributesAsync(string source) =>
			IsBoundAttributeAsync(source, 
											new List<DbBoundnessType>
											{
												DbBoundnessType.Unbound,
												DbBoundnessType.NotDefined,
												DbBoundnessType.Unbound,
												DbBoundnessType.DbBound,
												DbBoundnessType.NotDefined,
												DbBoundnessType.Unbound
											});

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public Task AreBoundAggregateAttributesAsync(string source) =>
			IsBoundAttributeAsync(source, new List<DbBoundnessType> { DbBoundnessType.DbBound, DbBoundnessType.Unbound });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public Task AreBoundAggregateRecursiveAttributeAsync(string source) =>
			IsBoundAttributeAsync(source, new List<DbBoundnessType> { DbBoundnessType.Unbound, DbBoundnessType.DbBound });

		private async Task IsBoundAttributeAsync(string source, List<DbBoundnessType> expected)
		{
			Document document = CreateDocument(source);
			var (semanticModel, syntaxRoot) = await document.GetSemanticModelAndRootAsync().ConfigureAwait(false);
			semanticModel.ThrowOnNull(nameof(semanticModel));
			syntaxRoot.ThrowOnNull(nameof(syntaxRoot));

			List<DbBoundnessType> actual = new List<DbBoundnessType>();
			var pxContext = new PXContext(semanticModel.Compilation, CodeAnalysisSettings.Default);
			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();
			var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.DbBoundnessCalculator(pxContext);

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();

				foreach (var attribute in attributes)
				{
					actual.Add(attributeInformation.GetBoundAttributeType(attribute));
				}
			}

			Assert.Equal(expected, actual);
		}

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldAttribute.cs")]
		public Task FieldBoundAttributesWithDynamicIsDBFieldSetInConstructorAsync(string source) =>
			IsDBFieldPropertyAsync(source,
								   new List<bool> { true, false });

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldWithDefinedAttributes.cs")]
		public Task FieldBoundAttributesWithDynamicIsDBFieldSetInAttributeDefinitionAsync(string source) =>
		   IsDBFieldPropertyAsync(source,
								  new List<bool> { false, false, false, false });

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldWithoutDefinedAttributes.cs", internalCodeFileNames: new string[] { @"ExternalAttributes1.cs", @"ExternalAttributes2.cs" })]
		public Task FieldBoundAttributesWithDynamicIsDBFieldSetInExternalAttributeDefinitionAsync(string source, string externalAttribute1, string externalAttribute2) =>
		   IsDBFieldPropertyAsync(source,
								  new List<bool> { false, false, true },
								  new string[] { externalAttribute1, externalAttribute2 });

		[Theory]
		[EmbeddedFileData(@"PropertyIsDBBoundFieldInheritedFromPXObjects.cs")]
		public Task IsDBFieldFromPXObjectsAsync(string source) =>
			IsDBFieldPropertyAsync(source,
									expected: new List<bool> {true, false, true, false});

		private async Task IsDBFieldPropertyAsync(string source, List<bool> expected, string[]? code = null)
		{
			Document document = CreateDocument(source, externalCode: code);
			var (semanticModel, syntaxRoot)  = await document.GetSemanticModelAndRootAsync().ConfigureAwait(false);
			semanticModel.ThrowOnNull(nameof(semanticModel));
			syntaxRoot.ThrowOnNull(nameof(syntaxRoot));

			List<bool> actual = new List<bool>(capacity: expected.Capacity);
			var pxContext = new PXContext(semanticModel.Compilation, CodeAnalysisSettings.Default);
			var dbBoundnessCalculator = new DbBoundnessCalculator(pxContext);

			IEnumerable<PropertyDeclarationSyntax> properties = syntaxRoot.DescendantNodes()
																		  .OfType<PropertyDeclarationSyntax>()
																		  .Where(a => !a.AttributeLists.IsNullOrEmpty());
			foreach (PropertyDeclarationSyntax property in properties)
			{
				IPropertySymbol? propertySymbol =  semanticModel.GetDeclaredSymbol(property);
				bool actualValue = IsPropertyDBBound(propertySymbol);
				actual.Add(actualValue);
			}

			actual.Should().BeEquivalentTo(expected);

			//-------------------------------------Local Function--------------------------------------
			bool IsPropertyDBBound(IPropertySymbol? propertySymbol)
			{
				if (propertySymbol == null) 
					return false;

				var propertyAttributes = propertySymbol.GetAttributes();

				if (propertyAttributes.IsDefaultOrEmpty)
					return false;

				return propertyAttributes.Any(a => dbBoundnessCalculator!.GetBoundAttributeType(a) == DbBoundnessType.DbBound);
			}
		}

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		private Task ListOfParentsSimpleAsync(string source)
		{
			return ListOfParentsAsync(source,
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
		private Task DacWithNonAcumaticaAttribute(string source)
		{
			return ListOfParentsAsync(source,
								new List<List<string>>
								{
									new List<string>{ "PX.Data.PXDBIntAttribute", },
									new List<string>{ "PX.Data.PXDBIntAttribute", }
								});
		}


		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private  Task ListOfParentsAggregateAsync(string source)
		{
			return ListOfParentsAsync(source,
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
		private Task ListOfParentsAggregateRecursiveAsync(string source)
		{
			return ListOfParentsAsync(source, new List<List<string>> {
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
		private Task ListOfParentsSimpleExpandedAsync(string source)
		{
			return ListOfParentsAsync(source,
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
		private Task ListOfParentsAggregateExpandedAsync(string source)
		{
			return ListOfParentsAsync(source,
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
		private Task ListOfParentsAggregateRecursiveExpandedAsync(string source)
		{
			return ListOfParentsAsync(source, new List<List<string>> {
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


		private async Task ListOfParentsAsync(string source, List<List<string>> expected, bool expand = false)
		{
			Document document = CreateDocument(source);
			var (semanticModel, syntaxRoot) = await document.GetSemanticModelAndRootAsync().ConfigureAwait(false);
			semanticModel.ThrowOnNull(nameof(semanticModel));
			syntaxRoot.ThrowOnNull(nameof(syntaxRoot));

			var pxContext = new PXContext(semanticModel.Compilation, CodeAnalysisSettings.Default);
			var expectedSymbols = ConvertSymbolNamesToTypeSymbols(expected, semanticModel);
			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			var actualResult = new List<IReadOnlyCollection<ITypeSymbol>>();

			foreach (var property in properties)
			{
				var propertySymbol = semanticModel.GetDeclaredSymbol(property);

				if (propertySymbol == null)
					continue;

				var attributes = propertySymbol.GetAttributes();

				foreach (var attribute in attributes)
				{
					var fullAttributesSet = attribute.AttributeClass.GetThisAndAllAggregatedAttributes(pxContext, expand);

					if (fullAttributesSet.Count > 0)
					{
						actualResult.Add(fullAttributesSet);
					}
				}
			}

			Assert.Equal(expectedSymbols, actualResult);
		}

		private List<HashSet<ITypeSymbol>> ConvertSymbolNamesToTypeSymbols(List<List<string>> expectedSymbolNamesSets, SemanticModel semanticModel) =>
			expectedSymbolNamesSets.Select(symbolNamesSet => GetTypeSymbolsFromNames(semanticModel, symbolNamesSet))
								   .ToList(capacity: expectedSymbolNamesSets.Count);

		private HashSet<ITypeSymbol> GetTypeSymbolsFromNames(SemanticModel semanticModel, List<string> symbolNames) =>
			symbolNames.Select(symbolName => semanticModel.Compilation.GetTypeByMetadataName(symbolName))
					   .Where(typeSymbol => typeSymbol != null)
					   .ToHashSet<ITypeSymbol>();
	}
}