using System;
using System.IO;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	internal class SuppressionFileWatcherService : ISuppressionFileWatcherService
	{
		private const int INSTANCE_UNDISPOSED = 0;
		private const int INSTANCE_DISPOSED = 1;
		private int _instanceDisposed = 0;

		private readonly FileSystemWatcher _fileSystemWatcher;

		public event FileSystemEventHandler Changed
		{
			add 
			{
				_fileSystemWatcher.Changed += value;
			}
			remove
			{
				_fileSystemWatcher.Changed -= value;
			}
		}

		public SuppressionFileWatcherService(FileSystemWatcher watcher)
		{
			_fileSystemWatcher = watcher.CheckIfNull(nameof(watcher));
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _instanceDisposed, INSTANCE_DISPOSED) == INSTANCE_UNDISPOSED)
			{			
				_fileSystemWatcher.Dispose();		
			}
		}	
	}
}
