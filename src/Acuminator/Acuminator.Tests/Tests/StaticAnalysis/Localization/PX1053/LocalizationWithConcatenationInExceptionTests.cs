using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class LocalizationWithConcatenationInExceptionTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationPXExceptionAnalyzer();

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
        [EmbeddedFileData(@"Localization\PX1053\LocalizationWithConcatenationInExceptions.cs",
                          @"Localization\Messages.cs")]
        public void Test_Localization_Of_PXException_With_Concatenations(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
                CreatePX1053DiagnosticResult(14, 75),
                CreatePX1053DiagnosticResult(21, 20));
        }
    }
}