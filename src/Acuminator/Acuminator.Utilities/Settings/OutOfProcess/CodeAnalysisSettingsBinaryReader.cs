using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Utilities.Settings.OutOfProcess
{
	public class CodeAnalysisSettingsBinaryReader : IDisposable
	{
		private bool _isDisposed;
		private readonly BinaryReader _reader;

		public CodeAnalysisSettingsBinaryReader(Stream stream)
		{
			_reader = new BinaryReader(stream);
		}

		public (CodeAnalysisSettings AnalysisSettings, BannedApiSettings BannedApiOptions) ReadAnalysisSettings()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(objectName: nameof(CodeAnalysisSettingsBinaryReader));

			var codeAnalysisSettings = ReadCodeAnalysisSettings();
			var bannedApiSetings = ReadBannedApiSettings();

			return (codeAnalysisSettings, bannedApiSetings);
		}

		private CodeAnalysisSettings ReadCodeAnalysisSettings()
		{
			bool recursiveAnalysisEnabled 			  = _reader.ReadBoolean();
			bool isvSpecificAnalyzersEnabled 		  = _reader.ReadBoolean();
			bool staticAnalysisEnabled 				  = _reader.ReadBoolean();
			bool suppressionMechanismEnabled 		  = _reader.ReadBoolean();
			bool px1007DocumentationDiagnosticEnabled = _reader.ReadBoolean();

			return new CodeAnalysisSettings(recursiveAnalysisEnabled, isvSpecificAnalyzersEnabled,
											staticAnalysisEnabled, suppressionMechanismEnabled,
											px1007DocumentationDiagnosticEnabled);
		}

		private BannedApiSettings ReadBannedApiSettings()
		{
			string bannedApiFilePath = _reader.ReadString();
			string whiteListApiFilePath = _reader.ReadString();

			return new BannedApiSettings(bannedApiFilePath, whiteListApiFilePath);
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				_reader.Dispose();
			}
		}
	}
}
