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
			VerifyCSharpDiagnostic(actual, CreatePX1042DiagnosticResult(18, 10));
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\CodeFixes\ConnectionScopeInRowSelecting_Expected.cs")]
		public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData(@"Event\PX1042\Diagnostics\ConnectionScopeInRowSelecting.cs",
			@"PXGraph\PX1042\CodeFixes\ConnectionScopeInRowSelecting_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
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
