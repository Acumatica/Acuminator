using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationInvocationCorrectTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationInvocationAnalyzer();

        [Theory]
        [EmbeddedFileData("LocalizationCorrect.cs",
                          "Messages.cs")]
        public void Test_Correct_Localization_Usage(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages });
        }
    }
}
