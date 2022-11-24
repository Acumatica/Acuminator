using System;
using System.Threading.Tasks;

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
											.WithSuppressionMechanismDisabled()
											.WithRecursiveAnalysisEnabled());

		[Theory]
		[EmbeddedFileData("LocalizationExceptionWithHardcodedStrings.cs")]
		public async Task Localization_PXException_WithHardcodedMessageArgument(string source)
		{
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(9, 45),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(16, 75),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(24, 20));
		}

		[Theory]
		[EmbeddedFileData("ExceptionWithConstStringFieldPassedToBaseConstructor.cs")]
		public async Task Localization_PXException_WithTypeMemberArguments_PassedToBaseConstructor(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1051_NonLocalizableString.CreateFor(21, 11),

				Descriptors.PX1051_NonLocalizableString.CreateFor(27, 11),
				Descriptors.PX1050_NonConstFieldStringInLocalizationMethod.CreateFor(27, 11),

				Descriptors.PX1051_NonLocalizableString.CreateFor(33, 11),
				Descriptors.PX1050_NonConstFieldStringInLocalizationMethod.CreateFor(33, 11),

				Descriptors.PX1051_NonLocalizableString.CreateFor(39, 11),
				Descriptors.PX1050_NonConstFieldStringInLocalizationMethod.CreateFor(39, 11));

		[Theory]
		[EmbeddedFileData("LocalizationWithNonLocalizableStringInExceptions.cs",
						  "Messages.cs")]
		public async Task Localization_PXException_With_NonLocalizableMessageArgument(string source, string messages)
		{
			await VerifyCSharpDiagnosticAsync(new[] { source, messages },
				Descriptors.PX1051_NonLocalizableString.CreateFor(14, 75),
				Descriptors.PX1051_NonLocalizableString.CreateFor(21, 20));
		}

		[Theory]
		[EmbeddedFileData("LocalizationInterpolationStringInException.cs")]
		public async Task Exception_With_InterpolationStrings(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(8, 19),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(20, 37));

		[Theory]
		[EmbeddedFileData("LocalizationNonLocalizationException.cs")]
		public async Task NonLocalization_Exception(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("LocalizationCorrect_MessagePassedFromAnotherPXException.cs")]
		public async Task MessagePassedFromAnotherPXException_Exception(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("LocalizationWithConcatenationInExceptions.cs",
						  "Messages.cs")]
		public async Task Localization_PXException_With_Concatenations(string source, string messages)
		{
			await VerifyCSharpDiagnosticAsync(new[] { source, messages },
				Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(14, 75),
				Descriptors.PX1053_ConcatenationPriorLocalization.CreateFor(21, 20));
		}
	}
}