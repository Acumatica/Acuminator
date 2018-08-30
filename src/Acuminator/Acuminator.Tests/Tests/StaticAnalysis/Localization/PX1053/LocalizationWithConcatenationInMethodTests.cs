using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class LocalizationWithConcatenationInMethodsTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationInvocationAnalyzer();

        private DiagnosticResult CreatePX1053DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1053_ConcatenationPriorLocalization.Id,
                Message = Descriptors.PX1053_ConcatenationPriorLocalization.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        [Theory]
        [EmbeddedFileData(@"Localization\PX1053\LocalizationWithConcatenationInMethods.cs",
                          @"Localization\Messages.cs")]
        public void Test_Localization_Methods_With_String_Concatenations(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
                CreatePX1053DiagnosticResult(13, 52),
                CreatePX1053DiagnosticResult(14, 52),
                CreatePX1053DiagnosticResult(15, 58),
                CreatePX1053DiagnosticResult(17, 51),
                CreatePX1053DiagnosticResult(18, 51),
                CreatePX1053DiagnosticResult(19, 59),
                CreatePX1053DiagnosticResult(21, 57),
                CreatePX1053DiagnosticResult(22, 57),
                CreatePX1053DiagnosticResult(23, 65),
                CreatePX1053DiagnosticResult(24, 68));
        }
    }
}
