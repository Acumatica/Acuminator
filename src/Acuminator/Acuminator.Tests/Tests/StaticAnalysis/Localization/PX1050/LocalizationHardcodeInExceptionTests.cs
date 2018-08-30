using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using DiagnosticVerifier = Acuminator.Tests.Verification.DiagnosticVerifier;

namespace Acuminator.Tests
{
    public class LocalizationHardcodeInExceptionTests : Verification.DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationPXExceptionAnalyzer();

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
        [EmbeddedFileData(@"Localization\PX1050\LocalizationExceptionWithHardcodedStrings.cs")]
        public void Test_Localization_Of_PXException_With_Hardcoded_Message_Argument(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1050DiagnosticResult(9, 45),
                CreatePX1050DiagnosticResult(16, 75),
                CreatePX1050DiagnosticResult(24, 20));
        }
    }
}