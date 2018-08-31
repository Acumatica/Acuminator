using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.StartRowResetForPaging;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.StartRowResetForPaging
{
	public class StartRowResetForPagingTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new StartRowResetForPagingAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new StartRowResetForPagingFix();


		[Theory]
		[EmbeddedFileData("StartRowResetForPaging.cs")]
		public void Test_StartRow_Reset_Diagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual, new[]
			{
				Descriptors.PX1010_StartRowResetForPaging.CreateFor(line: 20, column: 38),
				Descriptors.PX1010_StartRowResetForPaging.CreateFor(line: 48, column: 38),
				Descriptors.PX1010_StartRowResetForPaging.CreateFor(line: 65, column: 10)
			});
		}

		[Theory]
		[EmbeddedFileData("StartRowResetForPaging.cs",
						  "StartRowResetForPaging_Expected.cs")]
		public void Test_StartRow_Reset_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}
	}
}
