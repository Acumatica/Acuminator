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
		public void TestDiagnostic_Good(string actual) => VerifyCSharpDiagnostic(actual);

		[Theory]
		[EmbeddedFileData("LegacyBqlConstantBad.cs")]
		public void TestDiagnostic_Bad(string actual) => VerifyCSharpDiagnostic(actual,
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(10, 15, "LegacyBoolConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(15, 15, "LegacyByteConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(20, 15, "LegacyShortConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(25, 15, "LegacyIntConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(30, 15, "LegacyLongConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(35, 15, "LegacyFloatConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(40, 15, "LegacyDoubleConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(45, 15, "LegacyDecimalConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(50, 15, "LegacyStringConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(55, 15, "LegacyDateConst"),
			Descriptors.PX1061_LegacyBqlConstant.CreateFor(60, 15, "LegacyGuidConst"));

		[Theory]
		[EmbeddedFileData("LegacyBqlConstantBad.cs", "LegacyBqlConstantBad_Expected.cs")]
		public void TestCodeFix(string actual, string expected) => VerifyCSharpFix(actual, expected);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LegacyBqlConstantAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new LegacyBqlConstantFix();
	}
}