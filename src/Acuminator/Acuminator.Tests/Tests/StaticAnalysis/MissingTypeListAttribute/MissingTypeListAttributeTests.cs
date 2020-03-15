using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.MissingTypeListAttribute;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.MissingTypeListAttribute
{
    public class MissingTypeListAttributeTests : CodeFixVerifier
    {
        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeGood.cs")]
        public async Task TestDiagnostic_Good(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual);

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeInheritedList.cs")]
        public async Task TestDiagnostic_InheritedList_Good(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual);

        [Theory] 
        [EmbeddedFileData("MissingTypeListAttributeBad.cs")] 
        public async Task TestDiagnostic_Bad(string actual) => 
            await VerifyCSharpDiagnosticAsync(actual, 
                                              Descriptors.PX1002_MissingTypeListAttributeAnalyzer.CreateFor(14, 17));

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeBad.cs", "MissingTypeListAttributeBad_Expected.cs")]
        public async Task TestCodeFix(string actual, string expected) =>
            await VerifyCSharpFixAsync(actual, expected);

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new MissingTypeListAttributeAnalyzer());

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new MissingTypeListAttributeFix();
	}
}
