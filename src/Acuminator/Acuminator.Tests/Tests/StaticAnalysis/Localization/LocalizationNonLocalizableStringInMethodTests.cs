using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationNonLocalizableStringInMethodTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationInvocationAnalyzer();

        private DiagnosticResult CreatePX1051DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1051_NonLocalizableString.Id,
                Message = Descriptors.PX1051_NonLocalizableString.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        [Theory]
        [EmbeddedFileData("LocalizationWithNonLocalizableStringInMethods.cs",
                          "Messages.cs")]
        public void Test_Localization_Methods_With_Non_Localizable_Message_Argument(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
                CreatePX1051DiagnosticResult(11, 51),
                CreatePX1051DiagnosticResult(12, 51),
                CreatePX1051DiagnosticResult(13, 59),
                CreatePX1051DiagnosticResult(23, 57),
                CreatePX1051DiagnosticResult(24, 57),
                CreatePX1051DiagnosticResult(25, 65),
                CreatePX1051DiagnosticResult(26, 68),
                CreatePX1051DiagnosticResult(36, 52),
                CreatePX1051DiagnosticResult(37, 52),
                CreatePX1051DiagnosticResult(38, 58),
                CreatePX1051DiagnosticResult(39, 65));
        }
    }
}
