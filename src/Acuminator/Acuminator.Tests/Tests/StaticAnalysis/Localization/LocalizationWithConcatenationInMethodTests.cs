using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationWithConcatenationInMethodsTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationInvocationAnalyzer();

        [Theory]
        [EmbeddedFileData("LocalizationWithConcatenationInMethods.cs",
                          "Messages.cs")]
        public void Test_Localization_Methods_With_String_Concatenations(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(13, 52),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(14, 52),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(15, 58),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(17, 51),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(18, 51),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(19, 59),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(21, 57),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(22, 57),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(23, 65),
                Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(24, 68));
        }
    }
}
