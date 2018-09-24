using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries
{
	public class DatabaseQueriesInRowSelectingTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DatabaseQueriesInRowSelectingFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default.WithRecursiveAnalysisEnabled(),
				new DatabaseQueriesInRowSelectingAnalyzer());

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSelect.cs")]
		public Task TestDiagnostic_BQLSelect(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(19, 9));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\GenericEventHandlerSignature.cs")]
		public Task TestDiagnostic_GenericEventHandlerSignature(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(19, 9));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\DataView.cs")]
		public Task TestDiagnostic_DataView(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(32, 9));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXView.cs")]
		public Task TestDiagnostic_PXView(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(32, 9));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\NoNamespace.cs")]
		public Task TestDiagnostic_NoNamespace(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(18, 9));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSearch.cs")]
		public Task TestDiagnostic_BQLSearch(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(30, 9));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXSelector.cs")]
		public Task TestDiagnostic_PXSelector(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(20, 6));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXDatabase.cs")]
		public Task TestDiagnostic_PXDatabase(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(19, 9));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\ExternalMethod.cs")]
		public Task TestDiagnostic_ExternalMethod(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(18, 23));

		// TODO: Enable this test after migration to Roslyn v2
		[Theory(Skip = "IOperation feature (Operation Actions) is experimental in Roslyn v1")]
		[EmbeddedFileData(@"RowSelecting\Lambda.cs")]
		public Task TestDiagnostic_Lambda(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(14, 66));

		// TODO: Enable this test after migration to Roslyn v2
		[Theory(Skip = "IOperation feature (Operation Actions) is experimental in Roslyn v1")]
		[EmbeddedFileData(@"RowSelecting\LambdaWithBody.cs")]
		public Task TestDiagnostic_LambdaWithBody(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(18, 6));

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSelect_Expected.cs")]
		public Task TestDiagnostic_BQLSelect_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\GenericEventHandlerSignature_Expected.cs")]
		public Task TestDiagnostic_GenericEventHandlerSignature_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\DataView_Expected.cs")]
		public Task TestDiagnostic_DataView_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXView_Expected.cs")]
		public Task TestDiagnostic_PXView_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\NoNamespace_Expected.cs")]
		public Task TestDiagnostic_NoNamespace_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXSelector_Expected.cs")]
		public Task TestDiagnostic_PXSelector_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSearch_Expected.cs")]
		public Task TestDiagnostic_BQLSearch_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXDatabase_Expected.cs")]
		public Task TestDiagnostic_PXDatabase_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\ExternalMethod_Expected.cs")]
		public Task TestDiagnostic_ExternalMethod_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSelect.cs",
			@"RowSelecting\BQLSelect_Expected.cs")]
		public Task TestCodeFix_BQLSelect(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\GenericEventHandlerSignature.cs",
			@"RowSelecting\GenericEventHandlerSignature_Expected.cs")]
		public Task TestCodeFix_GenericEventHandlerSignature(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\DataView.cs",
			@"RowSelecting\DataView_Expected.cs")]
		public Task TestCodeFix_DataView(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXView.cs",
			@"RowSelecting\PXView_Expected.cs")]
		public Task TestCodeFix_PXView(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\NoNamespace.cs",
			@"RowSelecting\NoNamespace_Expected.cs")]
		public Task TestCodeFix_NoNamespace(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXSelector.cs",
			@"RowSelecting\PXSelector_Expected.cs")]
		public Task TestCodeFix_PXSelector(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSearch.cs",
			@"RowSelecting\BQLSearch_Expected.cs")]
		public Task TestCodeFix_BQLSearch(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXDatabase.cs",
			@"RowSelecting\PXDatabase_Expected.cs")]
		public Task TestCodeFix_PXDatabase(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"RowSelecting\ExternalMethod.cs",
			@"RowSelecting\ExternalMethod_Expected.cs")]
		public Task TestCodeFix_ExternalMethod(string actual, string expected) => VerifyCSharpFixAsync(actual, expected);
	}
}
