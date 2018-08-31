using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacNonAbstractFieldType
{
	public class DacFieldNotAbstractTests : DiagnosticVerifier
	{
		[Theory]
		[EmbeddedFileData("SOOrderNotAbstractField.cs")]
		public virtual void Test_Dac_With_Not_Abstract_Fields(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1024NotAbstractDacFieldDiagnosticResult(line: 22, column: 16),
				CreatePX1024NotAbstractDacFieldDiagnosticResult(line: 34, column: 16),
				CreatePX1024NotAbstractDacFieldDiagnosticResult(line: 45, column: 16));	

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacNonAbstractFieldTypeAnalyzer();

		
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
