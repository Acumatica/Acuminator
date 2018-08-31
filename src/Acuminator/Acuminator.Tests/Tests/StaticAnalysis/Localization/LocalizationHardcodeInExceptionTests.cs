using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationHardcodeInExceptionTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationPXExceptionAnalyzer();

        [Theory]
        [EmbeddedFileData("LocalizationExceptionWithHardcodedStrings.cs")]
        public void Test_Localization_Of_PXException_With_Hardcoded_Message_Argument(string source)
        {
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(9, 45),
	            Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(16, 75),
	            Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(24, 20));
        }
    }
}