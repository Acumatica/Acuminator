#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

using FluentAssertions;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Dac
{
	public class DacSemanticModelTests : SemanticModelTestsBase<DacSemanticModel>
	{
		[Theory]
		[EmbeddedFileData("RegularDac.cs")]
		public async Task RegularDac__DacFields_Recognition(string text)
		{
			var dacSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			// Check that fields from base types from PX.Objects were collected
			TestDacFields(dacSemanticModel, ["OrderType", "OrderNbr", "Status", "Tstamp"], fieldsCount: 4);

			foreach (var dacField in dacSemanticModel.DacFields)
			{
				dacField.BqlFieldInfo.Should().NotBeNull();
				dacField.BqlFieldInfo!.BqlFieldDataTypeDeclared.Should().BeNull();
				dacField.BqlFieldInfo.BqlFieldDataTypeEffective.Should().BeNull();

				dacField.PropertyInfo.Should().NotBeNull();
			}
		}

		[Theory]
		[EmbeddedFileData("DacFields_OnlyBqlFields.cs")]
		public async Task Dac_DerivedFromExternalDac_DacFields_Recognition(string text)
		{
			var dacSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			// Check that fields from base types from PX.Objects were collected
			TestDacFields(dacSemanticModel, ["docType", "refNbr", "noteID"], minFieldsCount: 4);

			foreach (var dacField in dacSemanticModel.DeclaredDacFields)
			{
				dacField.BqlFieldInfo.Should().NotBeNull();
				dacField.BqlFieldInfo!.BqlFieldDataTypeDeclared.Should().NotBeNull();
				
				dacField.PropertyInfo.Should().BeNull();
			}

			dacSemanticModel.BqlFieldsByNames["docType"].BqlFieldDataTypeDeclared!.Name.Should().Be("String");
			dacSemanticModel.BqlFieldsByNames["refNbr"].BqlFieldDataTypeDeclared!.Name.Should().Be("String");
			dacSemanticModel.BqlFieldsByNames["noteID"].BqlFieldDataTypeDeclared!.Name.Should().Be("Guid");
		}

		[Theory]
		[EmbeddedFileData("DacWithBaseTypeNonDac.cs")]
		public async Task Dac_DerivedFromNonDac_DacFieldsRecognition(string text)
		{
			var dacSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			TestDacFields(dacSemanticModel, requiredDacFields: ["OrderType", "OrderNbr", "CreatedByID", "CreatedByScreenID", "CreatedDateTime"], 
						  fieldsCount: 5);

			foreach (var dacField in dacSemanticModel.DacFields)
			{
				dacField.BqlFieldInfo.Should().NotBeNull();
				dacField.PropertyInfo.Should().NotBeNull();
			}
		}

		protected override Task<DacSemanticModel> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation = default)
		{
			var dacOrDacExtDeclaration = context.Root.DescendantNodes()
													 .OfType<ClassDeclarationSyntax>()
													 .FirstOrDefault();
			dacOrDacExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol? dacOrDacExtSymbol = context.SemanticModel.GetDeclaredSymbol(dacOrDacExtDeclaration);
			dacOrDacExtDeclaration.Should().NotBeNull();

			var dacModel = DacSemanticModel.InferModel(context.PXContext, dacOrDacExtSymbol!, cancellation: cancellation);
			dacModel.Should().NotBeNull();

			return Task.FromResult(dacModel!);
		}

		private void TestDacFields(DacSemanticModel dacModel, string[]? requiredDacFields = null, int? minFieldsCount = null,
									int? maxFieldsCount = null, int? fieldsCount = null)
		{
			if (minFieldsCount.HasValue)
				dacModel.DacFieldsByNames.Should().HaveCountGreaterOrEqualTo(minFieldsCount.Value);

			if (maxFieldsCount.HasValue)
				dacModel.DacFieldsByNames.Should().HaveCountLessOrEqualTo(maxFieldsCount.Value);

			if (fieldsCount.HasValue)
				dacModel.DacFieldsByNames.Should().HaveCount(fieldsCount.Value);

			if (requiredDacFields != null)
			{
				foreach (var requiredDacField in requiredDacFields)
					dacModel.DacFieldsByNames.Should().ContainKey(requiredDacField);
			}
		}
	}
}
