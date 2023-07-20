using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions
{
	public class NameConventionEventsInGraphsAndGraphExtensionsTests : CodeFixVerifier
	{
		protected override CodeRefactoringProvider GetCSharpCodeRefactoringProvider() => 
			new NameConventionEventsInGraphsAndGraphExtensionsAnalyzer(CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
																							.WithSuppressionMechanismDisabled());

		[Theory]
		[EmbeddedFileData("CacheAttached.cs", "CacheAttached_Expected.cs")]
		public Task CacheAttached(string actual, string expected)
		{
			return VerifyCSharpRefactoringAsync(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("RowEventHandler.cs", "RowEventHandler_Expected.cs")]
		public Task RowEventHandler(string actual, string expected)
		{
			return VerifyCSharpRefactoringAsync(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("FieldEventHandler.cs", "FieldEventHandler_Expected.cs")]
		public Task FieldEventHandler(string actual, string expected)
		{
			return VerifyCSharpRefactoringAsync(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("CacheAttachedWithArgUsages.cs", "CacheAttachedWithArgUsages_Expected.cs")]
		public Task CacheAttachedWithArgUsages(string actual, string expected)
		{
			return VerifyCSharpRefactoringAsync(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("EventHandlerWithArgsUsages.cs", "EventHandlerWithArgsUsages_Expected.cs")]
		public Task EventHandlerWithArgsUsages(string actual, string expected)
		{
			return VerifyCSharpRefactoringAsync(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("AdditionalParameters.cs")]
		public Task AdditionalParameters_ShouldNotSuggestRefactoring(string actual)
		{
			return VerifyCSharpRefactoringAsync(actual, actual, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("Override.cs")]
		public Task Override_ShouldNotSuggestRefactoring(string actual)
		{
			return VerifyCSharpRefactoringAsync(actual, actual, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("PXOverride.cs")]
		public Task PXOverride_ShouldNotSuggestRefactoring(string actual)
		{
			return VerifyCSharpRefactoringAsync(actual, actual, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}
	}
}
