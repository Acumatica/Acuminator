using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LegacyBqlField
{
	public class LegacyBqlFieldTest : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("LegacyBqlFieldGood.cs")]
		public void TestDiagnostic_Good(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}

		[Theory]
		[EmbeddedFileData("LegacyBqlFieldBad.cs")]
		public void TestDiagnostic_Bad(string actual)
		{
			VerifyCSharpDiagnostic(actual, Descriptors.PX1060_LegacyBqlField.CreateFor(12, 25, "legacyField"));
		}

		[Theory(Skip = "Bug in roslyn")]
		[EmbeddedFileData("LegacyBqlFieldBad.cs", "LegacyBqlFieldBad_Expected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new LegacyBqlFieldAnalyzer();
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new LegacyBqlFieldFix();
		}
	}
}