using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.NoPrimaryViewForPrimaryDac;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoPrimaryViewForPrimaryDac
{
	public class NoPrimaryViewForPrimaryDacTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			 new PXGraphAnalyzer(CodeAnalysisSettings.Default, 
								 new NoPrimaryViewForPrimaryDacAnalyzer());
			
		[Theory]
		[EmbeddedFileData("NoPrimaryViewForPrimaryDac.cs")]
		public virtual void Test_Graph_With_Primary_Dac_Without_Primary_View(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1018_NoPrimaryViewForPrimaryDac.CreateFor(line: 17, column: 56),
				Descriptors.PX1018_NoPrimaryViewForPrimaryDac.CreateFor(line: 23, column: 15));
	}
}
