using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class DacFieldAttributesTypeMismatchCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyAttributesAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new IncompatibleDacPropertyAndFieldAttributeFix();

		[Theory]
		[EmbeddedFileData("DacFieldAttributesTypeMismatch.cs",
						  "DacFieldAttributesTypeMismatch_Expected.cs")]
		public void Test_DAC_Property_Type_Not_Compatible_With_Field_Attribute_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}
	}
}
