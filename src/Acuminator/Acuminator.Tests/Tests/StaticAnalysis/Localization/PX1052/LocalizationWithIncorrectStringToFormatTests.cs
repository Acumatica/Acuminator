using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class LocalizationWithIncorrectStringToFormatTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationInvocationAnalyzer();

        private DiagnosticResult CreatePX1052DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1052_IncorrectStringToFormat.Id,
                Message = Descriptors.PX1052_IncorrectStringToFormat.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        [Theory]
        [EmbeddedFileData(@"Localization\PX1052\LocalizationWithIncorrectStringsToFormat.cs",
                          @"Localization\Messages.cs")]
        public void Test_Localization_Methods_With_Incorrect_Strings_To_Format(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
                CreatePX1052DiagnosticResult(12, 58),
                CreatePX1052DiagnosticResult(13, 57),
                CreatePX1052DiagnosticResult(14, 57),
                CreatePX1052DiagnosticResult(15, 65),
                CreatePX1052DiagnosticResult(16, 68));
        }
    }
}
