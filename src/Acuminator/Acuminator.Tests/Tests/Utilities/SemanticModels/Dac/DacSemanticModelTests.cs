#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;
using FluentAssertions;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Dac
{
	public class DacSemanticModelTests : SemanticModelTestsBase<DacSemanticModel>
	{
		[Theory]
		[EmbeddedFileData("DacFields_OnlyBqlFields.cs")]
		public async Task Test_DacFields_Recognition(string text)
		{
			var dacSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			dacSemanticModel.DacFieldsByNames.Should().HaveCountGreaterThan(3);		// Check that fields from base types from PX.Objects were collected
			dacSemanticModel.DacFieldsByNames.Should().ContainKey("docType");
			dacSemanticModel.DacFieldsByNames.Should().ContainKey("refNbr");
			dacSemanticModel.DacFieldsByNames.Should().ContainKey("noteID");
		}

		protected override Task<DacSemanticModel> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation = default)
		{
			var dacOrDacExtDeclaration = context.Root.DescendantNodes()
													 .OfType<ClassDeclarationSyntax>()
													 .FirstOrDefault();
			dacOrDacExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol dacOrDacExtSymbol = context.SemanticModel.GetDeclaredSymbol(dacOrDacExtDeclaration);
			dacOrDacExtDeclaration.Should().NotBeNull();

			var dacModel = DacSemanticModel.InferModel(context.PXContext, dacOrDacExtSymbol, cancellation: cancellation);
			dacModel.Should().NotBeNull();

			return Task.FromResult(dacModel!);
		}
	}
}
