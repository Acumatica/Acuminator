using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationNonLocalizableStringInExceptionTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationPXExceptionAnalyzer();

        [Theory]
        [EmbeddedFileData("LocalizationWithNonLocalizableStringInExceptions.cs",
                          "Messages.cs")]
        public void Test_Localization_Of_PXException_With_Non_Localizable_Message_Argument(string source, string messages)
        {
            VerifyCSharpDiagnostic(new[] { source, messages },
	            Descriptors.PX1051_NonLocalizableString.CreateFor(14, 75),
	            Descriptors.PX1051_NonLocalizableString.CreateFor(21, 20));
        }
    }
}
