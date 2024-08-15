#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

using Xunit;
using FluentAssertions;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels
{
	/// <summary>
	/// A vase class for semantic model tests with some shared logic.
	/// </summary>
	public abstract class SemanticModelTestsBase<TSemanticModel>
	where TSemanticModel : ISemanticModel
	{
		protected async Task<TSemanticModel> PrepareSemanticModelAsync(string code, CodeAnalysisSettings? customAnalysisSettings = null, 
																		CancellationToken cancellation = default)
		{
			var context = await PrepareTestContextForCodeAsync(code, customAnalysisSettings, cancellation).ConfigureAwait(false);
			var model = await PrepareSemanticModelAsync(context, cancellation).ConfigureAwait(false);
			return model;
		}

		protected abstract Task<TSemanticModel> PrepareSemanticModelAsync(RoslynTestContext context, CancellationToken cancellation = default);

		protected async Task<RoslynTestContext> PrepareTestContextForCodeAsync(string code, CodeAnalysisSettings? customAnalysisSettings = null, 
																			   CancellationToken cancellation = default)
		{
			code.Should().NotBeNullOrWhiteSpace();

			Document document = VerificationHelper.CreateDocument(code);

			var compilationTask   = document.Project.GetCompilationAsync(cancellation);
			var rootTask 		  = document.GetSyntaxRootAsync(cancellation);
			var semanticModelTask = document.GetSemanticModelAsync(cancellation);

			await Task.WhenAll(compilationTask, rootTask, semanticModelTask).ConfigureAwait(false);

#pragma warning disable VSTHRD103 // Call async methods when in an async method
			Compilation compilation 	 = compilationTask.Result;
			SyntaxNode? root 			 = rootTask.Result;
			SemanticModel? semanticModel = semanticModelTask.Result;
#pragma warning restore VSTHRD103 // Call async methods when in an async method

			compilation.Should().NotBeNull();
			semanticModel.Should().NotBeNull();
			root.Should().NotBeNull();
			CodeAnalysisSettings codeAnalysisSettings = customAnalysisSettings ?? CodeAnalysisSettings.Default
																									  .WithStaticAnalysisEnabled()
																									  .WithRecursiveAnalysisEnabled();
			PXContext pxContext = new PXContext(compilation, codeAnalysisSettings);
			return new RoslynTestContext(document, semanticModel, root, pxContext);
		}
	}
}
