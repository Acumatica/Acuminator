#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;

using FluentAssertions;

using PX.Common;

using Xunit;

namespace Acuminator.Tests.Tests.Utilities.CodeAnalysisSettingsSerialization
{
	public class SharedMemoryAccessTests
	{
		private const string EmptyStringPlaceHolder = "#";
		
		private const string ExternalExecutorFilePath = @".\..\..\ExternalRunner\App\ExternalRunner.exe";
		private const int Timeout_ThreeMinutes = 3 * 60_000;

		[Theory]
		[InlineData(true, true, true, true, true, false, null, null)]
		[InlineData(false, false, false, false, false, true, "", "    ")]
		[InlineData(false, true, false, false, true, true, @"C:\bannedApiPath.txt", @"C:\whiteListPath.txt")]
		[InlineData(true, false, false, true, true, false, null, @"C:\whiteListPath.txt")]
		[InlineData(true, false, false, true, true, true, @"C:\bannedApiPath.txt", null)]
		public void CheckAnalyzerReadSettingsCorrectly(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled,
													   bool staticAnalysisEnabled, bool suppressionMechanismEnabled, 
													   bool px1007DocumentationDiagnosticEnabled, bool bannedApiAnalysisEnabled,
													   string? bannedApiFilePath, string? whiteListFilePath)
		{
			string externalExecutorPath = Path.GetFullPath(ExternalExecutorFilePath);

			File.Exists(externalExecutorPath).Should().BeTrue();

			string externalExecutorDir = Path.GetDirectoryName(externalExecutorPath);
			var commandLineArgs = CreateCommandLineArgs(recursiveAnalysisEnabled, isvSpecificAnalyzersEnabled, 
														staticAnalysisEnabled, suppressionMechanismEnabled,
														px1007DocumentationDiagnosticEnabled, bannedApiAnalysisEnabled,
														bannedApiFilePath, whiteListFilePath);
			var expectedCodeAnalysisSettings = new CodeAnalysisSettings(recursiveAnalysisEnabled, isvSpecificAnalyzersEnabled,
																		staticAnalysisEnabled, suppressionMechanismEnabled, 
																		px1007DocumentationDiagnosticEnabled);
			var expectedBannedApiSettings = new BannedApiSettings(bannedApiAnalysisEnabled, bannedApiFilePath, whiteListFilePath);

			GlobalSettings.InitializeGlobalSettingsOnce(expectedCodeAnalysisSettings, expectedBannedApiSettings);
			var mockSettingsEvents = new MockSettingsEvents();

			using var outOfProcessSettingsUpdater = new OutOfProcessSettingsUpdater(mockSettingsEvents, expectedCodeAnalysisSettings, 
																					expectedBannedApiSettings, SharedVsSettings.SharedMemoryNameForTests);

			// Acuminator disable once PX1099 ForbiddenApiUsage
			var processStartInfo = new ProcessStartInfo(externalExecutorPath, commandLineArgs)
			{
				UseShellExecute  = false,
				WorkingDirectory = externalExecutorDir
			};

			using var process = Process.Start(processStartInfo);

			bool hasFinished = process.WaitForExit(Timeout_ThreeMinutes);
			
			hasFinished.Should().BeTrue();
			process.ExitCode.Should().Be(0);
		}

		private static string CreateCommandLineArgs(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled,
													bool staticAnalysisEnabled, bool suppressionMechanismEnabled,
													bool px1007DocumentationDiagnosticEnabled, bool bannedApiAnalysisEnabled,
													string? bannedApiFilePath, string? whiteListFilePath)
		{
			string[] commandLineArgsList =
			[
				recursiveAnalysisEnabled.ToString(),
				isvSpecificAnalyzersEnabled.ToString(),
				staticAnalysisEnabled.ToString(),
				suppressionMechanismEnabled.ToString(),
				px1007DocumentationDiagnosticEnabled.ToString(),
				bannedApiAnalysisEnabled.ToString(),
				bannedApiFilePath.NullIfWhitespace()?.Trim() ?? EmptyStringPlaceHolder,
				whiteListFilePath.NullIfWhitespace()?.Trim() ?? EmptyStringPlaceHolder
			];

			return commandLineArgsList.Join(" ");
		}
	}
}