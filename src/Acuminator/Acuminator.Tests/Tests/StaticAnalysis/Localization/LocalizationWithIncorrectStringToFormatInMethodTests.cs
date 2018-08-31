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

        [Theory]
        [EmbeddedFileData("LocalizationWithIncorrectStringToFormatInMethods.cs",
                          "Messages.cs")]
        public void Test_Localization_Methods_With_Incorrect_Strings_To_Format(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
	            Descriptors.PX1052_IncorrectStringToFormat.CreateFor(12, 58),
                Descriptors.PX1052_IncorrectStringToFormat.CreateFor(13, 57),
                Descriptors.PX1052_IncorrectStringToFormat.CreateFor(14, 57),
                Descriptors.PX1052_IncorrectStringToFormat.CreateFor(15, 65),
                Descriptors.PX1052_IncorrectStringToFormat.CreateFor(16, 68));
        }
    }
}
