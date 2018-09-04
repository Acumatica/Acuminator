using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationPXExceptionCorrectTests: DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationPXExceptionAnalyzer();

        [Theory]
        [EmbeddedFileData("LocalizationNonLocalizationException.cs")]
        public void Test_NonLocalization_Exception(string source)
        {
            VerifyCSharpDiagnostic(source);
        }
    }
}
