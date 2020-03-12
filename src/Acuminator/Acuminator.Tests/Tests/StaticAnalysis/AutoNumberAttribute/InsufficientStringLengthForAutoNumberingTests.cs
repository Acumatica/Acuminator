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
    public class InsufficientStringLengthForAutoNumberingTests : CodeFixVerifier
    {
        private const int NumberingSequenceStartNbrStringLength = 15;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new DacAnalyzersAggregator(
                CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
                                            .WithSuppressionMechanismDisabled(),
                new DacAutoNumberAttributeAnalyzer());

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new InsufficientStringLengthForAutoNumberingFix();

        [Theory]
        [EmbeddedFileData("InsufficientStringLengthDac.cs")]
        public async Task NonStringDAC_WithAutoNumbering(string source) =>
             await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1020_InsufficientStringLengthForDacPropertyWithAutoNumbering.CreateFor(18, 15, NumberingSequenceStartNbrStringLength));

        [Theory]
        [EmbeddedFileData("InsufficientStringLengthDac.cs", "InsufficientStringLengthDac_Expected.cs")]
        public async Task NonStringDAC_WithAutoNumbering_CodeFix(string actual, string expected) =>
            await VerifyCSharpFixAsync(actual, expected);
    }
}
