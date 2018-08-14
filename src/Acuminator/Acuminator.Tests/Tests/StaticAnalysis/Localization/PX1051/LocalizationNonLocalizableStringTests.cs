using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class LocalizationNonLocalizableStringTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationAnalyzer();

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
        [EmbeddedFileData(@"Localization\PX1051\LocalizationWithNonLocalizableStrings.cs",
                          @"Localization\Messages.cs")]
        public void Test_Localization_Methods_With_Non_Localizable_Message_Argument(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages });
        }
    }
}
