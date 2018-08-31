using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationNonLocalizableStringInMethodTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationInvocationAnalyzer();
		
        [Theory]
        [EmbeddedFileData("LocalizationWithNonLocalizableStringInMethods.cs",
                          "Messages.cs")]
        public void Test_Localization_Methods_With_Non_Localizable_Message_Argument(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
                Descriptors.PX1051_NonLocalizableString.CreateFor(11, 51),
                Descriptors.PX1051_NonLocalizableString.CreateFor(12, 51),
                Descriptors.PX1051_NonLocalizableString.CreateFor(13, 59),
                Descriptors.PX1051_NonLocalizableString.CreateFor(23, 57),
                Descriptors.PX1051_NonLocalizableString.CreateFor(24, 57),
                Descriptors.PX1051_NonLocalizableString.CreateFor(25, 65),
                Descriptors.PX1051_NonLocalizableString.CreateFor(26, 68),
                Descriptors.PX1051_NonLocalizableString.CreateFor(36, 52),
                Descriptors.PX1051_NonLocalizableString.CreateFor(37, 52),
                Descriptors.PX1051_NonLocalizableString.CreateFor(38, 58),
                Descriptors.PX1051_NonLocalizableString.CreateFor(39, 65));
        }
    }
}
