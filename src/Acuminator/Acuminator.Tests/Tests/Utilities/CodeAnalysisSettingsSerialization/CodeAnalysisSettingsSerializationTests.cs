using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Tests.Helpers;
using Acuminator.Utilities;
using Acuminator.Utilities.Settings.OutOfProcess;
using Acuminator.Utilities.Common;
using Xunit;
using FluentAssertions;


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
			var expectedSettings = new CodeAnalysisSettings(recursiveAnalysisEnabled, isvSpecificAnalyzersEnabled,
															staticAnalysisEnabled, suppressionMechanismEnabled, 
															px1007DocumentationDiagnosticEnabled);

			using var stream = new MemoryStream(capacity: sizeof(bool) * 5 + 20);
			using var writer = new CodeAnalysisSettingsBinaryWriter(stream);

			writer.WriteCodeAnalysisSettings(expectedSettings);

			stream.Position = 0;

			using var reader = new CodeAnalysisSettingsBinaryReader(stream);
			var deserializedSettings = reader.ReadCodeAnalysisSettings();

			Assert.Equal(deserializedSettings, expectedSettings);
		}
	}
}