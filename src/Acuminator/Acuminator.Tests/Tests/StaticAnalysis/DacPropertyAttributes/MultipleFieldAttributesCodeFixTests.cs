using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class MultipleFieldAttributesCodeFixTests : Verification.CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("DacWithMultipleFieldAttributes.cs",
						  "DacWithMultipleFieldAttributes_Expected.cs")]
		public void Test_Multiple_Field_Attributes_On_DAC_Property_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyAttributesAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MultipleDacFieldAttributesFix();
	}
}
