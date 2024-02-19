#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;
using FluentAssertions;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph
{
	public class GraphSemanticModelTests
	{
		[Theory]
		[EmbeddedFileData("GraphWithSetupViews.cs")]
		public async Task Test_SetupViews_Recognition(string text)
		{
			var (pxContext, graphSemanticModel) = await PrepareGraphModelAndAcumaticaContext(text).ConfigureAwait(false);

			foreach (var view in graphSemanticModel.Views)
			{
				view.IsSetup.Should().BeTrue();
			}
		}

		private async Task<(PXContext PXContext, PXGraphSemanticModel GraphSemanticModel)> PrepareGraphModelAndAcumaticaContext(string text)
		{
			text.Should().NotBeNullOrWhiteSpace();

			Document document = VerificationHelper.CreateDocument(text);

			var compilationTask   = document.Project.GetCompilationAsync();
			var rootTask 		  = document.GetSyntaxRootAsync();
			var semanticModelTask = document.GetSemanticModelAsync();

			await Task.WhenAll(compilationTask, rootTask, semanticModelTask).ConfigureAwait(false);

#pragma warning disable VSTHRD103 // Call async methods when in an async method
			Compilation compilation 	 = compilationTask.Result;
			SyntaxNode? root 			 = rootTask.Result;
			SemanticModel? semanticModel = semanticModelTask.Result;
#pragma warning restore VSTHRD103 // Call async methods when in an async method

			compilation.Should().NotBeNull();
			semanticModel.Should().NotBeNull();

			var graphOrGraphExtDeclaration = root?.DescendantNodes()
												  .OfType<ClassDeclarationSyntax>()
												  .FirstOrDefault();
			graphOrGraphExtDeclaration.Should().NotBeNull();

			INamedTypeSymbol? graphOrGraphExtSymbol = semanticModel.GetDeclaredSymbol(graphOrGraphExtDeclaration);
			graphOrGraphExtDeclaration.Should().NotBeNull();

			CodeAnalysisSettings codeAnalysisSettings = CodeAnalysisSettings.Default
																			.WithStaticAnalysisEnabled()
																			.WithRecursiveAnalysisEnabled();
			PXContext pxContext = new PXContext(compilation, codeAnalysisSettings);
			var graphSemanticModel = PXGraphSemanticModel.InferExplicitModel(pxContext, graphOrGraphExtSymbol, 
																			GraphSemanticModelCreationOptions.CollectGeneralGraphInfo, 
																			CancellationToken.None);
			graphSemanticModel.Should().NotBeNull();
			return (pxContext, graphSemanticModel!);
		}
	}
}
