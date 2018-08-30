using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.NoPrimaryViewForPrimaryDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoPrimaryViewForPrimaryDac
{
	public class NoPrimaryViewForPrimaryDacTests : DiagnosticVerifier
	{
		[Theory]
		[EmbeddedFileData("NoPrimaryViewForPrimaryDac.cs")]
		public virtual void Test_Graph_With_Primary_Dac_Without_Primary_View(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1018DiagnosticResult(line: 17, column: 56),
				CreatePX1018DiagnosticResult(line: 23, column: 15));

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new NoPrimaryViewForPrimaryDacAnalyzer();

		private DiagnosticResult CreatePX1018DiagnosticResult(int line, int column) =>
			new DiagnosticResult
			{
				Id = Descriptors.PX1018_NoPrimaryViewForPrimaryDac.Id,
				Message = Descriptors.PX1018_NoPrimaryViewForPrimaryDac.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", line, column)
				}
			};
	}
}
