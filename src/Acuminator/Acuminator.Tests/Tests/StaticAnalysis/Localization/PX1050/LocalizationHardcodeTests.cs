using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class LocalizationHardcodeTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationAnalyzer();

        private DiagnosticResult CreatePX1050DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1050_HardcodedStringInLocalizationMethod.Id,
                Message = Descriptors.PX1050_HardcodedStringInLocalizationMethod.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        [Theory]
        [EmbeddedFileData(@"Localization\PX1050\LocalizationWithHardcodedStrings.cs",
                          @"Localization\Messages.cs")]
        public void Test_Localization_Methods_With_Hardcoded_Message_Argument(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
                CreatePX1050DiagnosticResult(11, 51),
                CreatePX1050DiagnosticResult(12, 51),
                CreatePX1050DiagnosticResult(13, 59),
                CreatePX1050DiagnosticResult(23, 57),
                CreatePX1050DiagnosticResult(24, 57),
                CreatePX1050DiagnosticResult(25, 65),
                CreatePX1050DiagnosticResult(26, 68),
                CreatePX1050DiagnosticResult(36, 52),
                CreatePX1050DiagnosticResult(37, 52),
                CreatePX1050DiagnosticResult(38, 58),
                CreatePX1050DiagnosticResult(39, 65));
        }

        [Theory]
        [EmbeddedFileData(@"Localization\LocalizationCorrect.cs",
                          @"Localization\Messages.cs")]
        public void Test_Correct_Localization_Usage(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages });
        }
    }
}
