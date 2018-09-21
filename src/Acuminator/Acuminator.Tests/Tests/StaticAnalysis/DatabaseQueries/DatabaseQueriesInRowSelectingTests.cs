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
		public void TestDiagnostic_BQLSelect(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\GenericEventHandlerSignature.cs")]
		public void TestDiagnostic_GenericEventHandlerSignature(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\DataView.cs")]
		public void TestDiagnostic_DataView(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(32, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXView.cs")]
		public void TestDiagnostic_PXView(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(32, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\NoNamespace.cs")]
		public void TestDiagnostic_NoNamespace(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(18, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSearch.cs")]
		public void TestDiagnostic_BQLSearch(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(30, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXSelector.cs")]
		public void TestDiagnostic_PXSelector(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(20, 6));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXDatabase.cs")]
		public void TestDiagnostic_PXDatabase(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\ExternalMethod.cs")]
		public void TestDiagnostic_ExternalMethod(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(18, 23));
		}

		// TODO: Enable this test after migration to Roslyn v2
		[Theory(Skip = "IOperation feature (Operation Actions) is experimental in Roslyn v1")]
		[EmbeddedFileData(@"RowSelecting\Lambda.cs")]
		public void TestDiagnostic_Lambda(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(14, 66));
		}

		// TODO: Enable this test after migration to Roslyn v2
		[Theory(Skip = "IOperation feature (Operation Actions) is experimental in Roslyn v1")]
		[EmbeddedFileData(@"RowSelecting\LambdaWithBody.cs")]
		public void TestDiagnostic_LambdaWithBody(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_DatabaseQueriesInRowSelecting.CreateFor(18, 6));
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSelect_Expected.cs")]
		public void TestDiagnostic_BQLSelect_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\GenericEventHandlerSignature_Expected.cs")]
		public void TestDiagnostic_GenericEventHandlerSignature_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\DataView_Expected.cs")]
		public void TestDiagnostic_DataView_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXView_Expected.cs")]
		public void TestDiagnostic_PXView_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\NoNamespace_Expected.cs")]
		public void TestDiagnostic_NoNamespace_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXSelector_Expected.cs")]
		public void TestDiagnostic_PXSelector_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSearch_Expected.cs")]
		public void TestDiagnostic_BQLSearch_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXDatabase_Expected.cs")]
		public void TestDiagnostic_PXDatabase_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\ExternalMethod_Expected.cs")]
		public void TestDiagnostic_ExternalMethod_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSelect.cs",
			@"RowSelecting\BQLSelect_Expected.cs")]
		public void TestCodeFix_BQLSelect(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\GenericEventHandlerSignature.cs",
			@"RowSelecting\GenericEventHandlerSignature_Expected.cs")]
		public void TestCodeFix_GenericEventHandlerSignature(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\DataView.cs",
			@"RowSelecting\DataView_Expected.cs")]
		public void TestCodeFix_DataView(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXView.cs",
			@"RowSelecting\PXView_Expected.cs")]
		public void TestCodeFix_PXView(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\NoNamespace.cs",
			@"RowSelecting\NoNamespace_Expected.cs")]
		public void TestCodeFix_NoNamespace(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXSelector.cs",
			@"RowSelecting\PXSelector_Expected.cs")]
		public void TestCodeFix_PXSelector(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\BQLSearch.cs",
			@"RowSelecting\BQLSearch_Expected.cs")]
		public void TestCodeFix_BQLSearch(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\PXDatabase.cs",
			@"RowSelecting\PXDatabase_Expected.cs")]
		public void TestCodeFix_PXDatabase(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"RowSelecting\ExternalMethod.cs",
			@"RowSelecting\ExternalMethod_Expected.cs")]
		public void TestCodeFix_ExternalMethod(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}
	}
}
