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
		public async Task Graph_WithSetupViews_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			foreach (var view in graphSemanticModel.Views)
			{
				view.IsSetup.Should().BeTrue();
			}
		}

		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphWithExplicitInitializeMethod.cs")]
		public async Task Graph_WithExplicitInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraph);
			graphSemanticModel.InitializeMethodInfo.Should().NotBeNull();
		}

		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphWithImplicitInitializeMethod.cs")]
		public async Task Graph_WithImplicitInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraph);
			graphSemanticModel.InitializeMethodInfo.Should().NotBeNull();
		}
		
		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphWithIncorrectInitializeMethod.cs")]
		public async Task Graph_WithIncorrectInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraph);
			graphSemanticModel.InitializeMethodInfo.Should().BeNull();
		}

		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphExtensionWithInitializeMethod.cs")]
		public async Task GraphExtension_WithInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraphExtension);
			graphSemanticModel.InitializeMethodInfo.Should().NotBeNull();
		}

		[Theory]
		[EmbeddedFileData(@"InitializeMethod\GraphExtensionWithIncorrectInitializeMethod.cs")]
		public async Task GraphExtension_WithIncorrectInitialize_Recognition(string text)
		{
			var graphSemanticModel = await PrepareSemanticModelAsync(text).ConfigureAwait(false);

			graphSemanticModel.GraphType.Should().Be(GraphType.PXGraphExtension);
			graphSemanticModel.InitializeMethodInfo.Should().BeNull();
		}

		protected override Task<PXGraphSemanticModel> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation)
		{
			var graphOrGraphExtDeclaration = context.Root.DescendantNodes()
														 .OfType<ClassDeclarationSyntax>()
														 .FirstOrDefault();
			graphOrGraphExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol? graphOrGraphExtSymbol = context.SemanticModel.GetDeclaredSymbol(graphOrGraphExtDeclaration);
			graphOrGraphExtSymbol.Should().NotBeNull();

			var graphSemanticModel = PXGraphSemanticModel.InferExplicitModel(context.PXContext, graphOrGraphExtSymbol!,
																			GraphSemanticModelCreationOptions.CollectGeneralGraphInfo,
																			cancellation: cancellation);
			graphSemanticModel.Should().NotBeNull();
			return Task.FromResult(graphSemanticModel!);
		}
	}
}
