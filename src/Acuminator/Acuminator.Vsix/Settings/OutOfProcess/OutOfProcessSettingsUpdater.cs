using System;
using System.Collections.Generic;
using System.IO;
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
		private readonly MemoryMappedFile _memoryMappedFile;

		public OutOfProcessSettingsUpdater(ISettingsEvents settingsEvents, CodeAnalysisSettings initialSettings)
		{
			initialSettings.ThrowOnNull(nameof(initialSettings));

			_settingsEvents = settingsEvents.CheckIfNull(nameof(settingsEvents));
			_memoryMappedFile = CreateOrOpenMemoryMappedFile();

			WriteSettingsToSharedMemory(initialSettings);

			_settingsEvents.CodeAnalysisSettingChanged += SettingsEvents_CodeAnalysisSettingChanged;
		}

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _isDisposed, value: Disposed, comparand: NotDisposed) == NotDisposed)
			{
				if (_settingsEvents != null)
					_settingsEvents.CodeAnalysisSettingChanged -= SettingsEvents_CodeAnalysisSettingChanged;

				_memoryMappedFile?.Dispose();
			}
		}

		private void SettingsEvents_CodeAnalysisSettingChanged(object sender, SettingChangedEventArgs e)
		{
			var currentSettings = GlobalCodeAnalysisSettings.Instance;
			WriteSettingsToSharedMemory(currentSettings);
		}

		private MemoryMappedFile CreateOrOpenMemoryMappedFile()
		{
			const int estimatedMemorySizeInBytes = sizeof(bool) * 5 + 20; //Size of 5 flags + some additional memory just in case
			return MemoryMappedFile.CreateOrOpen(SharedVsSettings.AcuminatorSharedMemorySlotName, estimatedMemorySizeInBytes);
		}

		private void WriteSettingsToSharedMemory(CodeAnalysisSettings codeAnalysisSettings)
		{
			using MemoryMappedViewStream stream = _memoryMappedFile.CreateViewStream();
			using CodeAnalysisSettingsBinaryWriter writer = new CodeAnalysisSettingsBinaryWriter(stream);

			writer.WriteCodeAnalysisSettings(codeAnalysisSettings);
		}
	}
}
