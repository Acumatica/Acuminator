using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Settings.OutOfProcess;

namespace Acuminator.Analyzers.Settings.OutOfProcess
{
	internal static class AnalyzersOutOfProcessSettingsProvider
	{
		private static readonly object _lock = new object();
		private static volatile bool _isSharedMemoryOpened;

		private static MemoryMappedFile? _memoryMappedFile;

		public static CodeAnalysisSettings GetCodeAnalysisSettings(string? sharedMemoryName = null)
		{
			if (SharedVsSettings.IsInsideVsProcess)
				return GlobalSettings.AnalysisSettings;

			EnsureSharedMemoryIsOpened(sharedMemoryName);

			if (_memoryMappedFile == null)
				return GlobalSettings.AnalysisSettings;

			try
			{
				using MemoryMappedViewStream stream = _memoryMappedFile.CreateViewStream();
				using var reader = new CodeAnalysisSettingsBinaryReader(stream);

				CodeAnalysisSettings codeAnalysisSettings = reader.ReadCodeAnalysisSettings();

				return codeAnalysisSettings;
			}
			catch (Exception)
			{
				return GlobalSettings.AnalysisSettings;
			}
		}

		public static (CodeAnalysisSettings AnalysisSettings, BannedApiSettings BannedApiSettings) GetCodeAnalysisAndBannedApiSettings(
																										string? sharedMemoryName = null)
		{
			if (SharedVsSettings.IsInsideVsProcess)
				return (GlobalSettings.AnalysisSettings, GlobalSettings.BannedApiSettings);

			EnsureSharedMemoryIsOpened(sharedMemoryName);

			if (_memoryMappedFile == null)
				return (GlobalSettings.AnalysisSettings, GlobalSettings.BannedApiSettings);

			try
			{
				using MemoryMappedViewStream stream = _memoryMappedFile.CreateViewStream();
				using var reader = new CodeAnalysisSettingsBinaryReader(stream);

				var settings = reader.ReadAllAnalysisSettings();
				return settings;
			}
			catch (Exception)
			{
				return (GlobalSettings.AnalysisSettings, GlobalSettings.BannedApiSettings);
			}
		}

		private static void EnsureSharedMemoryIsOpened(string? sharedMemoryName)
		{
			if (!_isSharedMemoryOpened)
			{
				lock (_lock)
				{
					if (!_isSharedMemoryOpened)
					{
						_memoryMappedFile = OpenExistingMemoryMappedFile(sharedMemoryName);
						_isSharedMemoryOpened = _memoryMappedFile != null;
					}
				}
			}
		}

		private static MemoryMappedFile? OpenExistingMemoryMappedFile(string? sharedMemoryName)
		{
			try
			{
				string sharedMemorySlotName = sharedMemoryName.NullIfWhiteSpace()?.Trim() ?? SharedVsSettings.AcuminatorSharedMemorySlotName;
				return MemoryMappedFile.OpenExisting(sharedMemorySlotName);
			}
			catch (FileNotFoundException)
			{
				return null;
			}	
		}
	}
}
