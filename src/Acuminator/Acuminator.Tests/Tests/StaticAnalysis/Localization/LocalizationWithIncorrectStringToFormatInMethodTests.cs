using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationWithIncorrectStringToFormatInMethodTests : DiagnosticVerifier
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
        [EmbeddedFileData("LocalizationWithIncorrectStringToFormatInMethods.cs",
                          "Messages.cs")]
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
