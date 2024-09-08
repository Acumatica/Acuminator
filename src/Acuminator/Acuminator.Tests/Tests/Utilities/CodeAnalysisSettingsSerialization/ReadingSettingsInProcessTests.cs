#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.Utilities.CodeAnalysisSettingsSerialization
{
	[Collection(nameof(NotThreadSafeTestCollection))]
	public class ReadingSettingsInProcessTests
	{
		[Theory]
		[InlineData(true, true, true, true, true, false, null, null, "PlainGoodCode.cs")]
		[InlineData(false, false, false, false, false, true, "", "    ", "PlainGoodCode.cs")]
		[InlineData(false, true, false, false, true, true, @"C:\bannedApiPath.txt", @"C:\whiteListPath.txt", "PlainGoodCode.cs")]
		[InlineData(true, false, false, true, true, false, null, @"C:\whiteListPath.txt", "PlainGoodCode.cs")]
		[InlineData(true, false, false, true, true, true, @"C:\bannedApiPath.txt", null, "PlainGoodCode.cs")]
		public async Task CheckAnalyzerReadSettingsCorrectlyASync(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled,
																  bool staticAnalysisEnabled, bool suppressionMechanismEnabled,
																  bool px1007DocumentationDiagnosticEnabled, bool bannedApiAnalysisEnabled,
																  string? bannedApiFilePath, string? whiteListFilePath,
																  string sourceFileName)
		{
			var sourceCode = GetSourceCode(sourceFileName);

			sourceCode.Should().NotBeNullOrWhiteSpace();

			var expectedCodeAnalysisSettings = new CodeAnalysisSettings(recursiveAnalysisEnabled, isvSpecificAnalyzersEnabled,
																		staticAnalysisEnabled, suppressionMechanismEnabled,
																		px1007DocumentationDiagnosticEnabled);
			var expectedBannedApiSettings = new BannedApiSettings(bannedApiAnalysisEnabled, bannedApiFilePath, whiteListFilePath);

			GlobalSettings.InitializeGlobalSettingsThreadUnsafeForTestsOnly(expectedCodeAnalysisSettings, expectedBannedApiSettings);

			var analyzer = new ReadingFromMemoryAnalyzer(expectedCodeAnalysisSettings, expectedBannedApiSettings);
			var diagnosticVerifier = new DiagnosticVerifierWithPredefinedAnalyzer(analyzer);

			bool settingsReadCorrectly = await diagnosticVerifier.RunAnalyzerAndCheckIfItFinishedSuccessfullyAsync(sourceCode).ConfigureAwait(false);

			settingsReadCorrectly.Should().BeTrue("Analyzer should read settings correctly");
		}

		private static string GetSourceCode(string sourceFileName, [CallerFilePath] string? testFilePath = null)
		{
			string directory = Path.GetDirectoryName(testFilePath);
			string sourceFile = Path.Combine(directory, "Sources", sourceFileName);

			// Acuminator disable once PX1099 UsageOfForbiddenApi
			File.Exists(sourceFile).Should().BeTrue($"Source file \"{sourceFile}\" should exist");

			// Acuminator disable once PX1099 UsageOfForbiddenApi
			var sourceCode = File.ReadAllText(sourceFile);
			return sourceCode;
		}



		private class DiagnosticVerifierWithPredefinedAnalyzer : DiagnosticVerifier
		{
			private readonly ReadingFromMemoryAnalyzer _diagnosticAnalyzer;

			public DiagnosticVerifierWithPredefinedAnalyzer(ReadingFromMemoryAnalyzer diagnosticAnalyzer)
			{
				_diagnosticAnalyzer = diagnosticAnalyzer.CheckIfNull();
			}

			public async Task<bool> RunAnalyzerAndCheckIfItFinishedSuccessfullyAsync(string codeSource)
			{
				var analyzer	= GetCSharpDiagnosticAnalyzer();
				var diagnostics = await GetSortedDiagnosticsAsync([codeSource], LanguageNames.CSharp, analyzer, checkOnlyFirstDocument: true)
											.ConfigureAwait(false);
				return diagnostics.Length == 0;
			}

			protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => _diagnosticAnalyzer;
		}
	}
}