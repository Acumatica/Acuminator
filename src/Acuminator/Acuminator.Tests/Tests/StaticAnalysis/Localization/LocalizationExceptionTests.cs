using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Localization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.Localization
{
    public class LocalizationExceptionTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LocalizationPXExceptionAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

        [Theory]
        [EmbeddedFileData("LocalizationExceptionWithHardcodedStrings.cs")]
        public void Localization_PXException_WithHardcodedMessageArgument(string source)
        {
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(9, 45),
	            Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(16, 75),
	            Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(24, 20));
        }

		[Theory]
		[EmbeddedFileData("LocalizationWithNonLocalizableStringInExceptions.cs",
						  "Messages.cs")]
		public void Localization_PXException_With_NonLocalizableMessageArgument(string source, string messages)
		{
			VerifyCSharpDiagnostic(new[] { source, messages },
				Descriptors.PX1051_NonLocalizableString.CreateFor(14, 75),
				Descriptors.PX1051_NonLocalizableString.CreateFor(21, 20));
		}

		[Theory]
		[EmbeddedFileData("LocalizationNonLocalizationException.cs")]
		public void NonLocalization_Exception(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("LocalizationWithConcatenationInExceptions.cs",
						  "Messages.cs")]
		public void Localization_PXException_With_Concatenations(string source, string messages)
		{
			VerifyCSharpDiagnostic(new[] { source, messages },
				Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(14, 75),
				Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(21, 20));
		}
	}
}