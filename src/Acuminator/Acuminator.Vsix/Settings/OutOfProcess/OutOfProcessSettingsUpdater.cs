﻿#nullable enable

using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Settings.OutOfProcess;

namespace Acuminator.Vsix.Settings
{
	public class OutOfProcessSettingsUpdater : IDisposable
	{
		private const int NotDisposed = 0, Disposed = 1;
		private int _isDisposed = NotDisposed;
		
		private readonly string _sharedMemoryName;

		private readonly ISettingsEvents _settingsEvents;
		private readonly MemoryMappedFile _memoryMappedFile;

		public OutOfProcessSettingsUpdater(ISettingsEvents settingsEvents, CodeAnalysisSettings initialAnalysisSettings, 
										   BannedApiSettings initialBannedApiSettings, string? sharedMemoryName = null)
		{
			initialAnalysisSettings.ThrowOnNull();
			initialBannedApiSettings.ThrowOnNull();

			_sharedMemoryName = sharedMemoryName.NullIfWhiteSpace()?.Trim() ?? SharedVsSettings.AcuminatorSharedMemorySlotName;
			_settingsEvents   = settingsEvents.CheckIfNull();
			_memoryMappedFile = CreateOrOpenMemoryMappedFile();

			WriteSettingsToSharedMemory(initialAnalysisSettings, initialBannedApiSettings);

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

		private MemoryMappedFile CreateOrOpenMemoryMappedFile()
		{
			const int estimatedMemorySizeInBytes = sizeof(bool) * 5 + 20; //Size of 5 flags + some additional memory just in case
			return MemoryMappedFile.CreateOrOpen(_sharedMemoryName, estimatedMemorySizeInBytes);
		}

		private void SettingsEvents_CodeAnalysisSettingChanged(object sender, SettingChangedEventArgs e)
		{
			var currentAnalysisSettings = GlobalSettings.AnalysisSettings;
			var currentBannedApiSettings = GlobalSettings.BannedApiSettings;

			WriteSettingsToSharedMemory(currentAnalysisSettings, currentBannedApiSettings);
		}

		private void WriteSettingsToSharedMemory(CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings)
		{
			using MemoryMappedViewStream stream = _memoryMappedFile.CreateViewStream();
			using CodeAnalysisSettingsBinaryWriter writer = new CodeAnalysisSettingsBinaryWriter(stream);
			
			writer.WriteCodeAnalysisSettings(codeAnalysisSettings, bannedApiSettings);
		}
	}
}
