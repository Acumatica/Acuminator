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
        public void TestDiagnostic_Good(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeInheritedList.cs")]
        public void TestDiagnostic_InheritedList_Good(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeBad.cs")]
        public void TestDiagnostic_Bad(string actual)
        {
            VerifyCSharpDiagnostic(actual, Descriptors.PX1002_MissingTypeListAttributeAnalyzer.CreateFor(14, 17));
        }

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeBad.cs", "MissingTypeListAttributeBad_Expected.cs")]
        public void TestCodeFix(string actual, string expected)
        {
            VerifyCSharpFix(actual, expected);
        }

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new MissingTypeListAttributeAnalyzer());

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new MissingTypeListAttributeFix();
	}
}
