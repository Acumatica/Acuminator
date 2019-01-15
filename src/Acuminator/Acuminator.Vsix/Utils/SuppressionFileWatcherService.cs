using Acuminator.Utilities.DiagnosticSuppression;
using System;
using System.IO;

namespace Acuminator.Vsix.Utils
{
	internal class SuppressionFileWatcherService : ISuppressionFileWatcherService
	{
		public event Action<object, SuppressionFileEventArgs> Changed;

		public SuppressionFileWatcherService(FileSystemWatcher watcher)
		{
			watcher.Changed += OnChanged;
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			Changed?.Invoke(sender, new SuppressionFileEventArgs(e.FullPath, e.Name));
		}
	}
}
