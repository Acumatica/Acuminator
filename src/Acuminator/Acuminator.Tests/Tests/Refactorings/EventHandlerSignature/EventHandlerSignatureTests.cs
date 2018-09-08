using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.Refactorings.EventHandlerSignature;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Acuminator.Tests.Tests.Refactorings.EventHandlerSignature
{
	public class EventHandlerSignatureTests : CodeRefactoringVerifier
	{
		protected override CodeRefactoringProvider GetCSharpCodeRefactoringProvider() => new EventHandlerSignatureRefactoring();

		[Theory]
		[EmbeddedFileData("CacheAttached.cs", "CacheAttached_Expected.cs")]
		public void CacheAttached(string actual, string expected)
		{
			VerifyCSharpRefactoring(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("RowEventHandler.cs", "RowEventHandler_Expected.cs")]
		public void RowEventHandler(string actual, string expected)
		{
			VerifyCSharpRefactoring(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("FieldEventHandler.cs", "FieldEventHandler_Expected.cs")]
		public void FieldEventHandler(string actual, string expected)
		{
			VerifyCSharpRefactoring(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("CacheAttachedWithArgUsages.cs", "CacheAttachedWithArgUsages_Expected.cs")]
		public void CacheAttachedWithArgUsages(string actual, string expected)
		{
			VerifyCSharpRefactoring(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("EventHandlerWithArgsUsages.cs", "EventHandlerWithArgsUsages_Expected.cs")]
		public void EventHandlerWithArgsUsages(string actual, string expected)
		{
			VerifyCSharpRefactoring(actual, expected, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("AdditionalParameters.cs")]
		public void AdditionalParameters_ShouldNotSuggestRefactoring(string actual)
		{
			VerifyCSharpRefactoring(actual, actual, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("Override.cs")]
		public void Override_ShouldNotSuggestRefactoring(string actual)
		{
			VerifyCSharpRefactoring(actual, actual, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}

		[Theory]
		[EmbeddedFileData("PXOverride.cs")]
		public void PXOverride_ShouldNotSuggestRefactoring(string actual)
		{
			VerifyCSharpRefactoring(actual, actual, root => root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
		}
	}
}
