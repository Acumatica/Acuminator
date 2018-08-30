using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConnectionScopeInRowSelecting;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ConnectionScopeInRowSelecting
{
	public class ConnectionScopeInRowSelectingTests : Verification.CodeFixVerifier
	{
		private DiagnosticResult CreatePX1042DiagnosticResult(int line, int column)
		{
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1042_ConnectionScopeInRowSelecting.Id,
				Message = Descriptors.PX1042_ConnectionScopeInRowSelecting.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

			return diagnostic;
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelecting.cs")]
		public void TestDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(19, 9));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingGeneric.cs")]
		public void TestDiagnostic_GenericEventDeclaration(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(19, 9));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingDataView.cs")]
		public void TestDiagnostic_DataView(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(32, 9));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingPXView.cs")]
		public void TestDiagnostic_PXView(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(32, 9));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingNoNamespace.cs")]
		public void TestDiagnostic_NoNamespace(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(18, 9));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingSearch.cs")]
		public void TestDiagnostic_Search(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(30, 9));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingSelector.cs")]
		public void TestDiagnostic_Selector(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(20, 6));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingPXDatabase.cs")]
		public void TestDiagnostic_PXDatabase(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(19, 9));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingExternalMethod.cs")]
		public void TestDiagnostic_ExternalMethod(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(18, 23));
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelecting_Expected.cs")]
		public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingGeneric_Expected.cs")]
		public void TestDiagnostic_GenericEventDeclaration_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingDataView_Expected.cs")]
		public void TestDiagnostic_DataView_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingPXView_Expected.cs")]
		public void TestDiagnostic_PXView_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingNoNamespace_Expected.cs")]
		public void TestDiagnostic_NoNamespace_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingSelector_Expected.cs")]
		public void TestDiagnostic_Selector_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingSearch_Expected.cs")]
		public void TestDiagnostic_Search_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingPXDatabase_Expected.cs")]
		public void TestDiagnostic_PXDatabase_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingExternalMethod_Expected.cs")]
		public void TestDiagnostic_ExternalMethod_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelecting.cs",
			"ConnectionScopeInRowSelecting_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingGeneric.cs",
			"ConnectionScopeInRowSelectingGeneric_Expected.cs")]
		public void TestCodeFix_GenericEventDeclaration(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingDataView.cs",
			"ConnectionScopeInRowSelectingDataView_Expected.cs")]
		public void TestCodeFix_DataView(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingPXView.cs",
			"ConnectionScopeInRowSelectingPXView_Expected.cs")]
		public void TestCodeFix_PXView(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingNoNamespace.cs",
			"ConnectionScopeInRowSelectingNoNamespace_Expected.cs")]
		public void TestCodeFix_NoNamespace(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingSelector.cs",
			"ConnectionScopeInRowSelectingSelector_Expected.cs")]
		public void TestCodeFix_Selector(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingSearch.cs",
			"ConnectionScopeInRowSelectingSearch_Expected.cs")]
		public void TestCodeFix_Search(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingPXDatabase.cs",
			"ConnectionScopeInRowSelectingPXDatabase_Expected.cs")]
		public void TestCodeFix_PXDatabase(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ConnectionScopeInRowSelectingExternalMethod.cs",
			"ConnectionScopeInRowSelectingExternalMethod_Expected.cs")]
		public void TestCodeFix_ExternalMethod(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new ConnectionScopeInRowSelectingFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new ConnectionScopeInRowSelectingAnalyzer();
		}
	}
}
