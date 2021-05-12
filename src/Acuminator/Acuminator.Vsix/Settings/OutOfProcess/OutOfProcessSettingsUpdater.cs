using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Settings.OutOfProcess;

namespace Acuminator.Vsix.Settings
{
	internal class OutOfProcessSettingsUpdater : IDisposable
	{
		private const int NotDisposed = 0, Disposed = 1;
		private int _isDisposed = NotDisposed;
		
		private readonly ISettingsEvents _settingsEvents;
		private readonly ISettingsWriter _settingsWriter;

		public OutOfProcessSettingsUpdater(ISettingsEvents settingsEvents, ISettingsWriter settingsWriter, CodeAnalysisSettings initialSettings)
		{
			initialSettings.ThrowOnNull(nameof(initialSettings));

			_settingsEvents = settingsEvents.CheckIfNull(nameof(settingsEvents));
			_settingsWriter = settingsWriter.CheckIfNull(nameof(settingsWriter));

			_settingsEvents.CodeAnalysisSettingChanged += SettingsEvents_CodeAnalysisSettingChanged;

		}

		private void SettingsEvents_CodeAnalysisSettingChanged(object sender, SettingChangedEventArgs e)
		{
			var currentSettings = GlobalCodeAnalysisSettings.Instance;
		}

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _isDisposed, value: Disposed, comparand: NotDisposed) == NotDisposed)
			{
				if (_settingsEvents != null)
					_settingsEvents.CodeAnalysisSettingChanged -= SettingsEvents_CodeAnalysisSettingChanged;

				if (_settingsWriter is IDisposable disposableWriter)
					disposableWriter.Dispose();

				GC.SuppressFinalize(this);
			}
		}
	}
}
