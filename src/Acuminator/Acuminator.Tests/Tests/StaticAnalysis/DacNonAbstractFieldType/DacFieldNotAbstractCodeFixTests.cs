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
		[Theory]
		[EmbeddedFileData(@"SOOrderNotAbstractField.cs",
						  @"SOOrderNotAbstractField_Expected.cs")]
		public virtual void Test_Fix_For_Dac_With_Not_Abstract_Fields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacNonAbstractFieldTypeAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacNonAbstractFieldTypeFix();

		private DiagnosticResult CreatePX1024NotAbstractDacFieldDiagnosticResult(int line, int column)
		{
			return new DiagnosticResult
			{
				Id = Descriptors.PX1024_DacNonAbstractFieldType.Id,
				Message = Descriptors.PX1024_DacNonAbstractFieldType.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};
		}
	}
}
