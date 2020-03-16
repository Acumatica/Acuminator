using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.AutoNumberAttribute;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.AutoNumberAttribute
{
    public class NonStringDacPropertyWithAutoNumberingTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new DacAnalyzersAggregator(
                CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
                                            .WithSuppressionMechanismDisabled(),
                new DacAutoNumberAttributeAnalyzer());

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new NonStringDacPropertyWithAutoNumberingFix();

        [Theory]
        [EmbeddedFileData("NonStringAutoNumberingDac.cs")]
        public async Task NonStringDAC_WithAutoNumbering(string source) =>
            await VerifyCSharpDiagnosticAsync(source, 
                Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType.CreateFor(20, 4),
                Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType.CreateFor(22, 18));

        [Theory]
        [EmbeddedFileData("NonStringAutoNumberingDac.cs", "NonStringAutoNumberingDac_Expected.cs")]
        public async Task NonStringDAC_WithAutoNumbering_CodeFix(string actual, string expected) =>
            await VerifyCSharpFixAsync(actual, expected);
    }
}
