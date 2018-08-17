using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class LocalizationCorrectTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationInvocationAnalyzer();

        [Theory]
        [EmbeddedFileData(@"Localization\LocalizationCorrect.cs",
                          @"Localization\Messages.cs")]
        public void Test_Correct_Localization_Usage(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages });
        }
    }
}
