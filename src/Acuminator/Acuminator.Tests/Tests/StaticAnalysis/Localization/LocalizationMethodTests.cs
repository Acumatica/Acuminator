﻿using System;
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
	public class LocalizationMethodTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new LocalizationInvocationAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled()
											.WithRecursiveAnalysisEnabled());

		[Theory]
		[EmbeddedFileData("LocalizationMethodsWithHardcodedStrings.cs",
						  "Messages.cs")]
		public async Task Methods_WithHardcodedMessageArgument(string source, string messages)
		{
			await VerifyCSharpDiagnosticAsync(sources: new[] { source, messages },
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(11, 51),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(12, 51),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(13, 59),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(23, 57),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(24, 57),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(25, 65),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(26, 68),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(36, 52),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(37, 52),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(38, 58),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(39, 65),
				Descriptors.PX1050_HardcodedStringInLocalizationMethod.CreateFor(41, 43));
		}

		[Theory]
		[EmbeddedFileData("LocalizationWithIncorrectStringToFormatInMethods.cs",
						  "Messages.cs")]
		public async Task Methods_WithIncorrectStrings_ToFormat(string source, string messages)
		{
			await VerifyCSharpDiagnosticAsync(sources: new[] { source, messages },
				Descriptors.PX1052_IncorrectStringToFormat.CreateFor(12, 58),
				Descriptors.PX1052_IncorrectStringToFormat.CreateFor(13, 57),
				Descriptors.PX1052_IncorrectStringToFormat.CreateFor(14, 57),
				Descriptors.PX1052_IncorrectStringToFormat.CreateFor(15, 65),
				Descriptors.PX1052_IncorrectStringToFormat.CreateFor(16, 68));
		}

		[Theory]
		[EmbeddedFileData("LocalizationWithNonLocalizableStringInMethods.cs",
						  "Messages.cs")]
		public async Task Methods_WithNonLocalizableMessageArgument(string source, string messages)
		{
			await VerifyCSharpDiagnosticAsync(sources: new[] { source, messages },
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

		[Theory]
		[EmbeddedFileData("LocalizationWithNonConstStringsInMethods.cs",
						  "Messages.cs")]
		public async Task Methods_WithNonConstStringArgument(string source, string messages)
		{
			await VerifyCSharpDiagnosticAsync(sources: new[] { source, messages },
				Descriptors.PX1051_NonLocalizableString.CreateFor(12, 43),
				Descriptors.PX1050_NonConstFieldStringInLocalizationMethod.CreateFor(12, 43),
				Descriptors.PX1050_NonConstFieldStringInLocalizationMethod.CreateFor(14, 43),
				Descriptors.PX1050_NonConstFieldStringInLocalizationMethod.CreateFor(15, 43),
				Descriptors.PX1050_NonConstFieldStringInLocalizationMethod.CreateFor(17, 43),
				Descriptors.PX1050_NonConstFieldStringInLocalizationMethod.CreateFor(18, 43)
			);
		}

		[Theory]
		[EmbeddedFileData("LocalizationWithConcatenationInMethods.cs",
						  "Messages.cs")]
		public async Task Methods_WithStringConcatenations(string source, string messages)
		{
			await VerifyCSharpDiagnosticAsync(sources: new[] { source, messages },
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

		[Theory]
		[EmbeddedFileData("LocalizationCorrect.cs",
						  "Messages.cs")]
		public async Task CorrectLocalizationUsage_NoDiagnostic(string source, string messages) =>
			await VerifyCSharpDiagnosticAsync(sources: new[] { source, messages });
	}
}
