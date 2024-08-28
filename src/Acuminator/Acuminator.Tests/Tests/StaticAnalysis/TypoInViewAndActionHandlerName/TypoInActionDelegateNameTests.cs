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
    public class TypoInActionDelegateNameTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new TypoInViewOrActionDelegateNameFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			 new PXGraphAnalyzer(CodeAnalysisSettings.Default, 
				 new TypoInViewAndActionHandlerNameAnalyzer());

		[Theory]
        [EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_Good_SameName.cs")] 
		public Task RegularGraph_ActionAndActionDelegate_HaveSameName_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_Good_DifferentNames.cs")]
	    public Task RegularGraph_ActionAndActionDelegate_HaveTooDifferentNames_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_Good_Override.cs")]
	    public Task RegularGraph_ActionDelegateWithOverride_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
        [EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_Bad.cs")]
        public Task RegularGraph_TyposInActionDelegate(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1005_TypoInActionDelegateName.CreateFor(line: 16, column: 22, messageArgs: "Documents"));

		[Theory]
	    [EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_Bad.cs",
						  @"ActionDelegate\TypoInActionDelegateName_Bad_Expected.cs")]
		public Task RegularGraph_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_GraphExtension_Bad.cs")]
		public Task GraphExtension_TyposInBaseGraph_ActionInBaseGraph(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1005_TypoInActionDelegateName.CreateFor(line: 24, column: 22, messageArgs: "Documents"),
				Descriptors.PX1005_TypoInActionDelegateName.CreateFor(line: 34, column: 22, messageArgs: "ActionInBaseGraph"));

		[Theory]
		[EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_GraphExtension_Bad.cs",
						  @"ActionDelegate\TypoInActionDelegateName_GraphExtension_Bad_Expected.cs")]
		public Task GraphExtension_ActionDelegateTypos_ActionInBaseGraph_Rename(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_GraphExtension_Bad_Expected.cs")]
		public Task GraphExtension_AfterCodeFix_ShouldNotShowDiagnostic(string actual) => 
			 VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_DerivedGraph.cs")]
		public Task DerivedGraph_TyposInActionDelegates_ActionInBaseGraph(string actual) =>
					 VerifyCSharpDiagnosticAsync(actual,
						Descriptors.PX1005_TypoInActionDelegateName.CreateFor(line: 23, column: 22, messageArgs: "Documents"),
						Descriptors.PX1005_TypoInActionDelegateName.CreateFor(line: 33, column: 22, messageArgs: "ActionInBaseGraph"));

		[Theory]
		[EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_DerivedGraph.cs",
						  @"ActionDelegate\TypoInActionDelegateName_DerivedGraph_Expected.cs")]
		public Task DerivedGraph_ActionDelegateTypos_ActionInBaseGraph_Rename(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"ActionDelegate\TypoInActionDelegateName_DerivedGraph_Expected.cs")]
		public Task DerivedGraph_AfterCodeFix_ShouldNotShowDiagnostic(string actual) =>
			 VerifyCSharpDiagnosticAsync(actual);
	}
}
