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

		public void WriteCodeAnalysisSettings(CodeAnalysisSettings codeAnalysisSettings)
		{
			codeAnalysisSettings.ThrowOnNull(nameof(codeAnalysisSettings));

			if (_isDisposed)
				throw new ObjectDisposedException(objectName: nameof(CodeAnalysisSettingsBinaryWriter));

			_writer.Write(codeAnalysisSettings.RecursiveAnalysisEnabled);
			_writer.Write(codeAnalysisSettings.IsvSpecificAnalyzersEnabled);
			_writer.Write(codeAnalysisSettings.StaticAnalysisEnabled);
			_writer.Write(codeAnalysisSettings.SuppressionMechanismEnabled);
			_writer.Write(codeAnalysisSettings.PX1007DocumentationDiagnosticEnabled);
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
