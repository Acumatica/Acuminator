using System;
using System.IO;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;

namespace Acuminator.Vsix.Utilities
{
	internal class SuppressionFileWatcherService : ISuppressionFileWatcherService
	{
		private const int INSTANCE_UNDISPOSED = 0;
		private const int INSTANCE_DISPOSED = 1;
		private int _instanceDisposed = 0;

		private readonly FileSystemWatcher _fileSystemWatcher;

		public event Action<object, SuppressionFileEventArgs> Changed;

		public SuppressionFileWatcherService(FileSystemWatcher watcher)
		{
			_fileSystemWatcher = watcher.CheckIfNull(nameof(watcher));
			_fileSystemWatcher.Changed += OnChanged;
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			Changed?.Invoke(sender, new SuppressionFileEventArgs(e.FullPath, e.Name));
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _instanceDisposed, INSTANCE_DISPOSED) == INSTANCE_UNDISPOSED)
			{			
				_fileSystemWatcher.Changed -= OnChanged;
				_fileSystemWatcher.Dispose();		
			}
		}	
	}
}
