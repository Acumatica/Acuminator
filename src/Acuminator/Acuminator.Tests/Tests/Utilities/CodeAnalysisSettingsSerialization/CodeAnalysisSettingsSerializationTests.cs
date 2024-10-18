#nullable enable

using System;
using System.IO;

using Acuminator.Utilities;
using Acuminator.Utilities.Settings.OutOfProcess;
using Xunit;

namespace Acuminator.Tests.Tests.Utilities.CodeAnalysisSettingsSerialization
{

	public class CodeAnalysisSettingsSerializationTests
	{
		[Theory]
		[InlineData(true, true, true, true, true)]
		[InlineData(false, false, false, false, false)]
		[InlineData(false, true, false, false, true)]
		[InlineData(true, false, false, true, true)]
		public void CheckCodeAnalysisSettingsSerialization(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled, bool staticAnalysisEnabled,
														   bool suppressionMechanismEnabled, bool px1007DocumentationDiagnosticEnabled)
		{
			var expectedAnalysisSettings = new CodeAnalysisSettings(recursiveAnalysisEnabled, isvSpecificAnalyzersEnabled,
																	staticAnalysisEnabled, suppressionMechanismEnabled, 
																	px1007DocumentationDiagnosticEnabled);
			var bannedApiSettings = BannedApiSettings.Default;

			using var stream = new MemoryStream(capacity: sizeof(bool) * 5 + 20);
			using var writer = new CodeAnalysisSettingsBinaryWriter(stream);

			writer.WriteCodeAnalysisSettings(expectedAnalysisSettings, bannedApiSettings);

			stream.Position = 0;

			using var reader = new CodeAnalysisSettingsBinaryReader(stream);
			var deserializedSettings = reader.ReadCodeAnalysisSettings();

			Assert.Equal(deserializedSettings, expectedAnalysisSettings);
		}

		[Theory]
		[InlineData(true, true, true, true, true, false, null, null)]
		[InlineData(false, false, false, false, false, true, "", "    ")]
		[InlineData(false, true, false, false, true, true, @"C:\bannedApiPath.txt", @"C:\allowedApisPath.txt")]
		[InlineData(true, false, false, true, true, false, null, @"C:\allowedApisPath.txt")]
		[InlineData(true, false, false, true, true, true, @"C:\bannedApiPath.txt", null)]
		public void CheckCodeAnalysisAndBannedApiSettingsSerialization(bool recursiveAnalysisEnabled, bool isvSpecificAnalyzersEnabled,
																	   bool staticAnalysisEnabled, bool suppressionMechanismEnabled, 
																	   bool px1007DocumentationDiagnosticEnabled, bool bannedApiAnalysisEnabled,
																	   string? bannedApiFilePath, string? allowedApisFilePath)
		{
			var expectedAnalysisSettings = new CodeAnalysisSettings(recursiveAnalysisEnabled, isvSpecificAnalyzersEnabled,
																	staticAnalysisEnabled, suppressionMechanismEnabled,
																	px1007DocumentationDiagnosticEnabled);
			var expectedBannedApiSettings = new BannedApiSettings(bannedApiAnalysisEnabled, bannedApiFilePath, allowedApisFilePath);

			using var stream = new MemoryStream(capacity: sizeof(bool) * 5 + 20);
			using var writer = new CodeAnalysisSettingsBinaryWriter(stream);

			writer.WriteCodeAnalysisSettings(expectedAnalysisSettings, expectedBannedApiSettings);

			stream.Position = 0;

			using var reader = new CodeAnalysisSettingsBinaryReader(stream);
			var (deserializedAnalysisSettings, deserializedBannedApiSettings) = reader.ReadAllAnalysisSettings();

			Assert.Equal(deserializedAnalysisSettings, expectedAnalysisSettings);
			Assert.Equal(deserializedBannedApiSettings, expectedBannedApiSettings);
		}
	}
}