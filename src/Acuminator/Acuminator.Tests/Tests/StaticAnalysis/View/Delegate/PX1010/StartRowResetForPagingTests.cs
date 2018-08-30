using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.StartRowResetForPaging;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Microsoft.CodeAnalysis.CodeFixes;
using CodeFixVerifier = Acuminator.Tests.Verification.CodeFixVerifier;

namespace Acuminator.Tests
{
	public class StartRowResetForPagingTests : Verification.CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData(@"View\Delegate\PX1010\Diagnostics\StartRowResetForPaging.cs")]
		public void Test_StartRow_Reset_Diagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual, new[]
			{
				CreatePX1010DiagnosticResult(line: 20, column: 38),
				CreatePX1010DiagnosticResult(line: 48, column: 38),
				CreatePX1010DiagnosticResult(line: 65, column: 10)
			});
		}

		[Theory]
		[EmbeddedFileData(@"View\Delegate\PX1010\Diagnostics\StartRowResetForPaging.cs",
						  @"View\Delegate\PX1010\CodeFixes\StartRowResetForPaging_Expected.cs")]
		public void Test_StartRow_Reset_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new StartRowResetForPagingAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new StartRowResetForPagingFix();
		}

		private DiagnosticResult CreatePX1010DiagnosticResult(int line, int column) =>
			new DiagnosticResult
			{
				Id = Descriptors.PX1010_StartRowResetForPaging.Id,
				Message = Descriptors.PX1010_StartRowResetForPaging.Title.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", line, column)
				}
			};
	}
}
