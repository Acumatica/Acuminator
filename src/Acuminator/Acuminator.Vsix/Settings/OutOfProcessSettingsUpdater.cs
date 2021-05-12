using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Settings
{
	internal class OutOfProcessSettingsUpdater : IDisposable
	{
		private const int NotDisposed = 0, Disposed = 1;
		private int _isDisposed = NotDisposed;
		
		private readonly ISettingsEvents _settingsEvents;

		public OutOfProcessSettingsUpdater(ISettingsEvents settingsEvents)
		{
			_settingsEvents = settingsEvents.CheckIfNull(nameof(settingsEvents));

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
			}
		}
	}
}
