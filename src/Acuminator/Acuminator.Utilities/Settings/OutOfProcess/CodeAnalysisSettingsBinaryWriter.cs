using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Settings.OutOfProcess
{
	public class CodeAnalysisSettingsBinaryWriter : IDisposable
	{
		private bool _isDisposed;
		private readonly BinaryWriter _writer;

		public CodeAnalysisSettingsBinaryWriter(Stream stream)
		{
			_writer = new BinaryWriter(stream);
		}

		public void WriteCodeAnalysisSettings(CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings)
		{
			codeAnalysisSettings.ThrowOnNull();
			bannedApiSettings.ThrowOnNull();

			if (_isDisposed)
				throw new ObjectDisposedException(objectName: nameof(CodeAnalysisSettingsBinaryWriter));

			WriteAnalysisSettings(codeAnalysisSettings);
			WriteBannedApiSettings(bannedApiSettings);
		}

		private void WriteAnalysisSettings(CodeAnalysisSettings codeAnalysisSettings)
		{
			_writer.Write(codeAnalysisSettings.RecursiveAnalysisEnabled);
			_writer.Write(codeAnalysisSettings.IsvSpecificAnalyzersEnabled);
			_writer.Write(codeAnalysisSettings.StaticAnalysisEnabled);
			_writer.Write(codeAnalysisSettings.SuppressionMechanismEnabled);
			_writer.Write(codeAnalysisSettings.PX1007DocumentationDiagnosticEnabled);
		}

		private void WriteBannedApiSettings(BannedApiSettings bannedApiSettings)
		{
			_writer.Write(bannedApiSettings.BannedApiAnalysisEnabled);

			string bannedApiFilePath = bannedApiSettings.BannedApiFilePath ?? string.Empty;
			_writer.Write(bannedApiFilePath);

			string whiteListApiFilePath = bannedApiSettings.WhiteListApiFilePath ?? string.Empty;
			_writer.Write(whiteListApiFilePath);
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				_writer.Dispose();
			}
		}
	}
}
