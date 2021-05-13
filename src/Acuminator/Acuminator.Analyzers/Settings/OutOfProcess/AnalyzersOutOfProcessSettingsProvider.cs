using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Settings.OutOfProcess;


namespace Acuminator.Analyzers.Settings.OutOfProcess
{
	internal static class AnalyzersOutOfProcessSettingsProvider
	{
		public static CodeAnalysisSettings GetCodeAnalysisSettings()
		{
			if (SharedVsSettings.IsInsideVsProcess)
				return GlobalCodeAnalysisSettings.Instance;

			var memoryMappedFile = OpenExistingMemoryMappedFile();
			
			if (memoryMappedFile == null)
				return GlobalCodeAnalysisSettings.Instance;

			using MemoryMappedViewStream stream = memoryMappedFile.CreateViewStream();
			using var reader = new CodeAnalysisSettingsBinaryReader(stream);
			CodeAnalysisSettings codeAnalysisSettings =	reader.ReadCodeAnalysisSettings();
			return codeAnalysisSettings;
		}

		private static MemoryMappedFile OpenExistingMemoryMappedFile()
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
