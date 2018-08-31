using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;


namespace Acuminator.Tests.Tests.StaticAnalysis.DacNonAbstractFieldType
{
	public class DacFieldNotAbstractCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacNonAbstractFieldTypeAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacNonAbstractFieldTypeFix();

		[Theory]
		[EmbeddedFileData(@"SOOrderNotAbstractField.cs",
						  @"SOOrderNotAbstractField_Expected.cs")]
		public virtual void Test_Fix_For_Dac_With_Not_Abstract_Fields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}
