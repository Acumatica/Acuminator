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

		public static CodeAnalysisSettings GetCodeAnalysisSettings()
		{
			if (SharedVsSettings.IsInsideVsProcess)
				return GlobalSettings.AnalysisSettings;

			EnsureSharedMemoryIsOpened();

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

		public static (CodeAnalysisSettings AnalysisSettings, BannedApiSettings BannedApiSettings) GetCodeAnalysisAndBannedApiSettings()
		{
			if (SharedVsSettings.IsInsideVsProcess)
				return (GlobalSettings.AnalysisSettings, GlobalSettings.BannedApiSettings);

			EnsureSharedMemoryIsOpened();

			if (_memoryMappedFile == null)
				return (GlobalSettings.AnalysisSettings, GlobalSettings.BannedApiSettings);

			try
			{
				using MemoryMappedViewStream stream = _memoryMappedFile.CreateViewStream();
				using var reader = new CodeAnalysisSettingsBinaryReader(stream);

				var settings = reader.ReadAnalysisSettings();
				return settings;
			}
			catch (Exception)
			{
				return (GlobalSettings.AnalysisSettings, GlobalSettings.BannedApiSettings);
			}
		}

		private static void EnsureSharedMemoryIsOpened()
		{
			if (!_isSharedMemoryOpened)
			{
				lock (_lock)
				{
					if (!_isSharedMemoryOpened)
					{
						_memoryMappedFile = OpenExistingMemoryMappedFile();
						_isSharedMemoryOpened = _memoryMappedFile != null;
					}
				}
			}
		}

		private static MemoryMappedFile? OpenExistingMemoryMappedFile()
		{
			try
			{
				return MemoryMappedFile.OpenExisting(SharedVsSettings.AcuminatorSharedMemorySlotName);
			}
			catch (FileNotFoundException)
			{
				return null;
			}	
		}
	}
}
