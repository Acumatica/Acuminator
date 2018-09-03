using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConnectionScopeInRowSelecting;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ConnectionScopeInRowSelecting
{
	public class ConnectionScopeInRowSelectingTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new ConnectionScopeInRowSelectingFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new EventHandlerAnalyzer(new ConnectionScopeInRowSelectingAnalyzer());

		[Theory]
		[EmbeddedFileData("BQLSelect.cs")]
		public void TestDiagnostic_BQLSelect(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData("GenericEventHandlerSignature.cs")]
		public void TestDiagnostic_GenericEventHandlerSignature(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData("DataView.cs")]
		public void TestDiagnostic_DataView(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(32, 9));
		}

		[Theory]
		[EmbeddedFileData("PXView.cs")]
		public void TestDiagnostic_PXView(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(32, 9));
		}

		[Theory]
		[EmbeddedFileData("NoNamespace.cs")]
		public void TestDiagnostic_NoNamespace(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(18, 9));
		}

		[Theory]
		[EmbeddedFileData("BQLSearch.cs")]
		public void TestDiagnostic_BQLSearch(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(30, 9));
		}

		[Theory]
		[EmbeddedFileData("PXSelector.cs")]
		public void TestDiagnostic_PXSelector(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(20, 6));
		}

		[Theory]
		[EmbeddedFileData("PXDatabase.cs")]
		public void TestDiagnostic_PXDatabase(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(19, 9));
		}

		[Theory]
		[EmbeddedFileData("ExternalMethod.cs")]
		public void TestDiagnostic_ExternalMethod(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(18, 23));
		}

		[Theory]
		[EmbeddedFileData("Lambda.cs")]
		public void TestDiagnostic_Lambda(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1042_ConnectionScopeInRowSelecting.CreateFor(14, 77));
		}

		[Theory]
		[EmbeddedFileData("BQLSelect_Expected.cs")]
		public void TestDiagnostic_BQLSelect_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("GenericEventHandlerSignature_Expected.cs")]
		public void TestDiagnostic_GenericEventHandlerSignature_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("DataView_Expected.cs")]
		public void TestDiagnostic_DataView_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("PXView_Expected.cs")]
		public void TestDiagnostic_PXView_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("NoNamespace_Expected.cs")]
		public void TestDiagnostic_NoNamespace_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("PXSelector_Expected.cs")]
		public void TestDiagnostic_PXSelector_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("BQLSearch_Expected.cs")]
		public void TestDiagnostic_BQLSearch_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("PXDatabase_Expected.cs")]
		public void TestDiagnostic_PXDatabase_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("ExternalMethod_Expected.cs")]
		public void TestDiagnostic_ExternalMethod_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("BQLSelect.cs",
			"BQLSelect_Expected.cs")]
		public void TestCodeFix_BQLSelect(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("GenericEventHandlerSignature.cs",
			"GenericEventHandlerSignature_Expected.cs")]
		public void TestCodeFix_GenericEventHandlerSignature(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("DataView.cs",
			"DataView_Expected.cs")]
		public void TestCodeFix_DataView(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("PXView.cs",
			"PXView_Expected.cs")]
		public void TestCodeFix_PXView(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("NoNamespace.cs",
			"NoNamespace_Expected.cs")]
		public void TestCodeFix_NoNamespace(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("PXSelector.cs",
			"PXSelector_Expected.cs")]
		public void TestCodeFix_PXSelector(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("BQLSearch.cs",
			"BQLSearch_Expected.cs")]
		public void TestCodeFix_BQLSearch(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("PXDatabase.cs",
			"PXDatabase_Expected.cs")]
		public void TestCodeFix_PXDatabase(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ExternalMethod.cs",
			"ExternalMethod_Expected.cs")]
		public void TestCodeFix_ExternalMethod(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}
	}
}
