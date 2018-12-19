using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlConstant;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LegacyBqlConstant
{
	public class LegacyBqlConstantTest : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("LegacyBqlConstantGood.cs")]
		public void TestDiagnostic_Good(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("LegacyBqlConstantBad.cs")]
		public void TestDiagnostic_Bad(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1061_LegacyBqlConstant.CreateFor(10, 15, "LegacyConst"));
		}

		[Theory(Skip = "Bug in roslyn")]
		[EmbeddedFileData("LegacyBqlConstantBad.cs", "LegacyBqlConstantBad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new LegacyBqlConstantAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new LegacyBqlConstantFix();
		}
	}
}