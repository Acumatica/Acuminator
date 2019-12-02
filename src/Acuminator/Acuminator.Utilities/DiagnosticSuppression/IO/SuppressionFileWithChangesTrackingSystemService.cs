using System;
using System.IO;
using System.Xml.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// A service for accessing suppression file with tracking of suppression file changes.
	/// </summary>
	internal class SuppressionFileWithChangesTrackingSystemService : SuppressionFileSystemServiceBase
	{
		public SuppressionFileWithChangesTrackingSystemService() : 
														  base(null, null)
		{
		}

		public SuppressionFileWithChangesTrackingSystemService(IIOErrorProcessor errorProcessor) : 
														  base(errorProcessor, null)
		{
		}

		public SuppressionFileWithChangesTrackingSystemService(IIOErrorProcessor errorProcessor, SuppressionFileValidation customValidation) : 
														  base(errorProcessor, customValidation)
		{
		}

		public override ISuppressionFileWatcherService CreateWatcher(string path)
		{
			var directory = Path.GetDirectoryName(path);
			var file = Path.GetFileName(path);
			var watcher = new FileSystemWatcher(directory, file)
			{
				NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
				EnableRaisingEvents = true
			};

			return new SuppressionFileWatcherService(watcher);
		}
	}
}