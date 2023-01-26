#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

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
			CheckIfAttributesOnPropertiesAreDerivedFromPXDefaultAsync(source, new List<bool> { true, false, false });

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
					actual.Add(attribute.AttributeClass.IsDerivedFromOrAggregatesAttribute(pxdefaultAttribute, pxContext));
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
												DbBoundnessType.NotDefined,
												DbBoundnessType.PXDBCalced,
												DbBoundnessType.NotDefined,
												DbBoundnessType.NotDefined
											});

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public Task AreBoundAggregateAttributesAsync(string source) =>
			IsBoundAttributeAsync(source, new List<DbBoundnessType> { DbBoundnessType.DbBound, DbBoundnessType.NotDefined });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public Task AreBoundAggregateRecursiveAttributeAsync(string source) =>
			IsBoundAttributeAsync(source, new List<DbBoundnessType> { DbBoundnessType.Unbound, DbBoundnessType.DbBound, DbBoundnessType.Unknown });

		private async Task IsBoundAttributeAsync(string source, List<DbBoundnessType> expected)
		{
			Document document = CreateDocument(source);
			var (semanticModel, syntaxRoot) = await document.GetSemanticModelAndRootAsync().ConfigureAwait(false);
			semanticModel.ThrowOnNull(nameof(semanticModel));
			syntaxRoot.ThrowOnNull(nameof(syntaxRoot));

			var actual = new List<DbBoundnessType>(capacity: expected.Count);
			var pxContext = new PXContext(semanticModel.Compilation, CodeAnalysisSettings.Default);
			var dbBoundnessCalculator = new DbBoundnessCalculator(pxContext);
			var types = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();

			foreach (var type in types)
			{
				INamedTypeSymbol typeSymbol = semanticModel.GetDeclaredSymbol(type).CheckIfNull(nameof(typeSymbol));

				if (!typeSymbol.IsDacOrExtension(pxContext))
					continue;

				var properties = type.DescendantNodes().OfType<PropertyDeclarationSyntax>();

				foreach (var property in properties)
				{
					var propertySymbol = semanticModel.GetDeclaredSymbol(property);
					var attributes = propertySymbol.GetAttributes();

					foreach (var attribute in attributes)
					{
						actual.Add(dbBoundnessCalculator.GetAttributeApplicationDbBoundnessType(attribute));
					}
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

				return propertyAttributes.Any(a => dbBoundnessCalculator!.GetAttributeApplicationDbBoundnessType(a) == DbBoundnessType.DbBound);
			}
		}

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		private Task FlattenedAttributesSets_SimpleDac_NoBaseTypes(string source) =>
			CompareFlattenedAttributesSetsAsync(source,
				new[]
				{
					new[] { "PX.Data.PXBoolAttribute" },
					new[] { "PX.Data.PXDefaultAttribute" },
					new[] { "PX.Data.PXUIFieldAttribute" },
					new[] { "PX.Data.PXDBCalcedAttribute" },
					new[] { "PX.Data.PXDefaultAttribute"},
					new[] { "PX.Data.PXUIFieldAttribute"}
				},
				includeBaseTypes: false);

		[Theory]
		[EmbeddedFileData(@"NotAcumaticaAttributeDac.cs")]
		private Task FlattenedAttributesSets_DacWithNonAcumaticaAttribute_NoBaseTypes(string source) =>
			CompareFlattenedAttributesSetsAsync(source,
				new[]
				{
					new[] { "PX.Data.PXDBIntAttribute", },
					new[] { "PX.Data.PXDBIntAttribute", }
				},
				includeBaseTypes: false);

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private  Task FlattenedAttributesSets_AggregatesOnAggregates_NoBaseTypes(string source) =>
			CompareFlattenedAttributesSetsAsync(source,
				new[] {
					new[]
					{
						"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
						"PX.Objects.HackathonDemo.NonNullableIntAttribute",
						"PX.Data.PXDBIntAttribute",
						"PX.Data.PXDefaultAttribute",
						"PX.Data.PXIntListAttribute"
					},
					new[]
					{
						"PX.Objects.HackathonDemo.PXAccountAttribute",
						"PX.Objects.HackathonDemo.PXCustomDefaultAttribute"
					}
				},
				includeBaseTypes: false);

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		private Task FlattenedAttributesSets_AggregateWithRecursion_NoBaseTypes(string source) =>
			CompareFlattenedAttributesSetsAsync(source,
				new[]
				{
					new[]
					{
						"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
						"PX.Objects.HackathonDemo.NonNullableIntAttribute",
						"PX.Data.PXDefaultAttribute",
						"PX.Data.PXIntListAttribute"
					},
					new[]
					{
						"PX.Objects.HackathonDemo._NonNullableIntListAttribute",
						"PX.Objects.HackathonDemo._NonNullableIntAttribute",
						"PX.Data.PXDBIntAttribute",
						"PX.Data.PXIntListAttribute"
					}
				},
				includeBaseTypes: false);

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		private Task FlattenedAttributesSets_SimpleDac_WithBaseTypes(string source) =>
			CompareFlattenedAttributesSetsAsync(source,
				new[]
				{
					new[] { "PX.Data.PXBoolAttribute" },
					new[] { "PX.Data.PXDefaultAttribute" },
					new[] { "PX.Data.PXUIFieldAttribute" },
					new[] { "PX.Data.PXDBCalcedAttribute" },
					new[] { "PX.Data.PXDefaultAttribute" },
					new[] { "PX.Data.PXUIFieldAttribute" }
				},
				includeBaseTypes: true);

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private Task FlattenedAttributesSets_AggregatesOnAggregates_WithBaseTypes(string source) =>
			CompareFlattenedAttributesSetsAsync(source,
				new[]
				{
					new[]
					{
						"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
						"PX.Objects.HackathonDemo.NonNullableIntAttribute",
						"PX.Data.PXDBIntAttribute",
						"PX.Data.PXDBFieldAttribute",
						"PX.Data.PXDefaultAttribute",
						"PX.Data.PXIntListAttribute"
					},
					new[]
					{
						"PX.Objects.HackathonDemo.PXAccountAttribute",
						"PX.Objects.HackathonDemo.PXCustomDefaultAttribute",
						"PX.Data.PXDefaultAttribute"
					}
				},
				includeBaseTypes: true);

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		private Task FlattenedAttributesSets_AggregateWithRecursion_WithBaseTypes(string source) =>
			CompareFlattenedAttributesSetsAsync(source,
				new[]
				{
					new[]
					{
						"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
						"PX.Objects.HackathonDemo.NonNullableIntAttribute",
						"PX.Data.PXDefaultAttribute",
						"PX.Data.PXIntListAttribute"
					},
					new[]
					{
						"PX.Objects.HackathonDemo._NonNullableIntListAttribute",
						"PX.Objects.HackathonDemo._NonNullableIntAttribute",
						"PX.Data.PXDBIntAttribute",
						"PX.Data.PXDBFieldAttribute",
						"PX.Data.PXIntListAttribute"
					}
				},
				includeBaseTypes: true);


		private async Task CompareFlattenedAttributesSetsAsync(string source, string[][] expectedFlattenedSets, bool includeBaseTypes)
		{
			Document document = CreateDocument(source);
			var (semanticModel, syntaxRoot) = await document.GetSemanticModelAndRootAsync().ConfigureAwait(false);
			semanticModel.ThrowOnNull(nameof(semanticModel));
			syntaxRoot.ThrowOnNull(nameof(syntaxRoot));

			var expectedSymbols = ConvertSymbolNamesToTypeSymbols(expectedFlattenedSets, semanticModel);
			var actualResult = new List<List<ITypeSymbol>>(expectedSymbols.Count);

			var pxContext = new PXContext(semanticModel.Compilation, CodeAnalysisSettings.Default);
			var types = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();

			foreach (var type in types)
			{
				INamedTypeSymbol typeSymbol = semanticModel.GetDeclaredSymbol(type).CheckIfNull(nameof(typeSymbol));

				if (!typeSymbol.IsDacOrExtension(pxContext))
					continue;

				var properties = type.DescendantNodes().OfType<PropertyDeclarationSyntax>();

				foreach (var property in properties)
				{
					if (semanticModel.GetDeclaredSymbol(property) is not IPropertySymbol propertySymbol)
						continue;

					var attributes = propertySymbol.GetAttributes();

					foreach (var attribute in attributes)
					{
						ImmutableHashSet<ITypeSymbol> fullAttributesSet = attribute.AttributeClass.GetThisAndAllAggregatedAttributes(pxContext, includeBaseTypes);

						if (fullAttributesSet.Count > 0)
						{
							var actualSet = fullAttributesSet.OrderBy(t => t.ToString()).ToList(fullAttributesSet.Count);
							actualResult.Add(actualSet);
						}
					}
				}
			}

			Assert.Equal(expectedSymbols, actualResult);
		}

		private List<List<ITypeSymbol>> ConvertSymbolNamesToTypeSymbols(string[][] expectedSymbolNamesSets, SemanticModel semanticModel) =>
			expectedSymbolNamesSets.Select(symbolNamesSet => GetTypeSymbolsFromNames(semanticModel, symbolNamesSet))
								   .ToList(capacity: expectedSymbolNamesSets.Length);

		private List<ITypeSymbol> GetTypeSymbolsFromNames(SemanticModel semanticModel, IEnumerable<string> symbolNames) =>
			symbolNames.Select(symbolName => semanticModel.Compilation.GetTypeByMetadataName(symbolName))
					   .Where(typeSymbol => typeSymbol != null)
					   .Distinct()
					   .OrderBy(typeSymbol => typeSymbol.ToString())	
					   .ToList<ITypeSymbol>();
	}
}