#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;
using FluentAssertions;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph
{
	public class GraphSemanticModelTests : SemanticModelTestsBase<PXGraphSemanticModel>
	{
		[Theory]
		[EmbeddedFileData("GraphWithSetupViews.cs")]
		public async Task Test_SetupViews_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			foreach (var view in graphSemanticModel.Views)
			{
				view.IsSetup.Should().BeTrue();
			}
		}

		protected override Task<PXGraphSemanticModel> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation)
		{
			var graphOrGraphExtDeclaration = context.Root.DescendantNodes()
														 .OfType<ClassDeclarationSyntax>()
														 .FirstOrDefault();
			graphOrGraphExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol graphOrGraphExtSymbol = context.SemanticModel.GetDeclaredSymbol(graphOrGraphExtDeclaration);
			graphOrGraphExtSymbol.Should().NotBeNull();

			var graphSemanticModel = PXGraphSemanticModel.InferExplicitModel(context.PXContext, graphOrGraphExtSymbol,
																			GraphSemanticModelCreationOptions.CollectGeneralGraphInfo,
																			cancellation);
			graphSemanticModel.Should().NotBeNull();
			return Task.FromResult(graphSemanticModel!);
		}
	}
}
