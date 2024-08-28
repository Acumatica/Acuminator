#nullable enable

using System;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.TypoInViewAndActionHandlerName;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.TypoInViewAndActionDelegateName
{
    public class TypoInViewAndActionHandlerNameTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new TypoInViewOrActionDelegateNameFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			 new PXGraphAnalyzer(CodeAnalysisSettings.Default, 
				 new TypoInViewAndActionHandlerNameAnalyzer());

		[Theory]
        [EmbeddedFileData(@"ViewDelegate\TypoInViewDelegateName_Good_SameName.cs")] 
		public Task RegularGraph_ViewAndViewDelegate_HaveSameName_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData(@"ViewDelegate\TypoInViewDelegateName_Good_DifferentNames.cs")]
	    public Task RegularGraph_ViewAndViewDelegate_HaveTooDifferentNames_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData(@"ViewDelegate\TypoInViewDelegateName_Good_Override.cs")]
	    public Task RegularGraph_ViewDelegateWithOverride_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
        [EmbeddedFileData(@"ViewDelegate\TypoInViewDelegateName_Bad.cs")]
        public Task RegularGraph_TyposInViewDelegate(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1005_TypoInViewDelegateName.CreateFor(line: 16, column: 22, messageArgs: "Documents"));

		[Theory]
	    [EmbeddedFileData(@"ViewDelegate\TypoInViewDelegateName_Bad.cs",
						  @"ViewDelegate\TypoInViewDelegateName_Bad_Expected.cs")]
		public Task RegularGraph_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"ViewDelegate\TypoInViewDelegateName_GraphExtension_Bad.cs")]
		public Task TestDiagnostic_GraphExtension(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1005_TypoInViewDelegateName.CreateFor(line: 23, column: 22, messageArgs: "ViewInBaseGraph"),
				Descriptors.PX1005_TypoInViewDelegateName.CreateFor(line: 35, column: 22, messageArgs: "Documents"),
				Descriptors.PX1005_TypoInViewDelegateName.CreateFor(line: 45, column: 22, messageArgs: "ViewInBaseGraph"));

		[Theory]
		[EmbeddedFileData(@"ViewDelegate\TypoInViewDelegateName_GraphExtension_Bad_Expected.cs")]
		public Task GraphExtension_ViewDelegateTypos_ShouldNotShowDiagnostic(string actual) => 
			 VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"ViewDelegate\TypoInViewDelegateName_GraphExtension_Bad.cs",
						  @"ViewDelegate\TypoInViewDelegateName_GraphExtension_Bad_Expected.cs")]
		public Task GraphExtension_ViewDelegateTypos_CodeFix_Rename(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);
	}
}
