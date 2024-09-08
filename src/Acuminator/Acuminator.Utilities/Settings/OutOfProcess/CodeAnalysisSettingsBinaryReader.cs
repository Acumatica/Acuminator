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

		public (CodeAnalysisSettings AnalysisSettings, BannedApiSettings BannedApiOptions) ReadAllAnalysisSettings()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(objectName: nameof(CodeAnalysisSettingsBinaryReader));

			var codeAnalysisSettings = ReadCodeAnalysisSettingsImpl();
			var bannedApiSetings = ReadBannedApiSettings();

			return (codeAnalysisSettings, bannedApiSetings);
		}

		public CodeAnalysisSettings ReadCodeAnalysisSettings()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(objectName: nameof(CodeAnalysisSettingsBinaryReader));

			return ReadCodeAnalysisSettingsImpl();
		}

		private CodeAnalysisSettings ReadCodeAnalysisSettingsImpl()
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
			bool isBannedApiAnalysisEnabled = _reader.ReadBoolean();
			string bannedApiFilePath 		= _reader.ReadString();
			string whiteListApiFilePath 	= _reader.ReadString();

			return new BannedApiSettings(isBannedApiAnalysisEnabled, bannedApiFilePath, whiteListApiFilePath);
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
