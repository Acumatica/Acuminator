using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.Analyzers;
using Acuminator.Analyzers.FixProviders;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
	public class ConnectionScopeInRowSelectingTests : CodeFixVerifier
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
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelecting.cs")]
		public void TestDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(19, 9));
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelectingGeneric.cs")]
		public void TestDiagnostic_GenericEventDeclaration(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(19, 9));
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelectingDataView.cs")]
		public void TestDiagnostic_DataView(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(32, 9));
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelectingPXView.cs")]
		public void TestDiagnostic_PXView(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(32, 9));
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelectingNoNamespace.cs")]
		public void TestDiagnostic_NoNamespace(string actual)
		{
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(18, 9));
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelecting_Expected.cs")]
		public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelectingGeneric_Expected.cs")]
		public void TestDiagnostic_GenericEventDeclaration_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelectingDataView_Expected.cs")]
		public void TestDiagnostic_DataView_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelectingPXView_Expected.cs")]
		public void TestDiagnostic_PXView_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelectingNoNamespace_Expected.cs")]
		public void TestDiagnostic_NoNamespace_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelecting.cs",
			@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelecting_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelectingGeneric.cs",
			@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelectingGeneric_Expected.cs")]
		public void TestCodeFix_GenericEventDeclaration(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelectingDataView.cs",
			@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelectingDataView_Expected.cs")]
		public void TestCodeFix_DataView(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelectingPXView.cs",
			@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelectingPXView_Expected.cs")]
		public void TestCodeFix_PXView(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelectingNoNamespace.cs",
			@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelectingNoNamespace_Expected.cs")]
		public void TestCodeFix_NoNamespace(string actual, string expected)
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
