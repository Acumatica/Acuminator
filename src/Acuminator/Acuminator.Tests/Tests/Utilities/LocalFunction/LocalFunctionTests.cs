using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities.Roslyn.Semantic;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels
{
	[SuppressMessage("Acuminator", "PX1120:Incorrect work with Task types in the Acumatica asynchronous code. Task-typed expressions should be awaited.",
					 Justification = "<Pending>")]
	public class LocalFunctionTests
	{
		[Theory]
		[EmbeddedFileData("StaticLocalFunction.cs")]
		public async Task Static_LocalFunctions_Have_StaticSymbols(string text)
		{
			var (document, semanticModel, root) = await PrepareTestSolutionAsync(text).ConfigureAwait(false);
			var classDeclaration = root.DescendantNodes()
									   .OfType<ClassDeclarationSyntax>()
									   .FirstOrDefault();
			classDeclaration.Should().NotBeNull();

			var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

			foreach (var methodNode in methods)
			{
				var localFunctions = methodNode.DescendantNodes()
											   .OfType<LocalFunctionStatementSyntax>();

				foreach (var localFunction in localFunctions)
				{
					var symbol = semanticModel.GetDeclaredSymbol(localFunction, default) as IMethodSymbol;
					symbol.Should().NotBeNull();

					symbol!.MethodKind.Should().Be(MethodKind.LocalFunction);
					bool isStatic = symbol.Name.StartsWith("Static", StringComparison.OrdinalIgnoreCase);

					symbol.IsStatic.Should().Be(isStatic);
				}
			}
		}

		[Theory]
		[EmbeddedFileData("StaticLambda.cs")]
		public async Task Static_Lambdas_Have_StaticSymbols(string text)
		{
			var (document, semanticModel, root) = await PrepareTestSolutionAsync(text).ConfigureAwait(false);
			var classDeclaration = root.DescendantNodes()
									   .OfType<ClassDeclarationSyntax>()
									   .FirstOrDefault();
			classDeclaration.Should().NotBeNull();

			var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

			foreach (MethodDeclarationSyntax methodNode in methods)
			{
				string methodName = methodNode.Identifier.ValueText;
				bool isStatic 	  = methodName.StartsWith("Static", StringComparison.OrdinalIgnoreCase);
				var lambdas 	  = methodNode.DescendantNodes()
											  .OfType<AnonymousFunctionExpressionSyntax>();

				foreach (var lambda in lambdas)
				{
					var symbol = semanticModel.GetSymbolOrBestCandidate(lambda, default) as IMethodSymbol;
					symbol.Should().NotBeNull();

					symbol!.MethodKind.Should().Be(MethodKind.LambdaMethod);
					symbol.IsStatic.Should().Be(isStatic);
				}
			}
		}

		protected async Task<(Document Document, SemanticModel SemanticModel, SyntaxNode Root)> PrepareTestSolutionAsync(string code, 
																											CancellationToken cancellation = default)
		{
			code.Should().NotBeNullOrWhiteSpace();

			Document document = SolutionBuilder.CreateDocument(code);

			var compilationTask   = document.Project.GetCompilationAsync(cancellation);
			var rootTask 		  = document.GetSyntaxRootAsync(cancellation);
			var semanticModelTask = document.GetSemanticModelAsync(cancellation);

			await Task.WhenAll(compilationTask, rootTask, semanticModelTask).ConfigureAwait(false);

#pragma warning disable VSTHRD103 // Call async methods when in an async method
			Compilation? compilation 	 = compilationTask.Result;
			SyntaxNode? root 			 = rootTask.Result;
			SemanticModel? semanticModel = semanticModelTask.Result;
#pragma warning restore VSTHRD103 // Call async methods when in an async method

			compilation.Should().NotBeNull();
			semanticModel.Should().NotBeNull();
			root.Should().NotBeNull();
			
			return (document, semanticModel!, root!);
		}
	}
}
