using System;
using System.IO;
using System.Security;
using System.Xml.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;

namespace Acuminator.Vsix.Utilities
{
	internal class SuppressionFileSystemService : ISuppressionFileSystemService
	{
		public XDocument Load(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			try
			{
				return XDocument.Load(path);
			}
			catch (SecurityException)
			{
			}
			catch (IOException)
			{
			}

			return null;
		}

		public bool Save(XDocument document, string path)
		{
			document.ThrowOnNull(nameof(document));
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			try
			{
				document.Save(path);
			}
			catch (SecurityException)
			{
				return false;
			}
			catch (IOException)
			{
				return false;
			}

			return true;
		}

		public string GetFileName(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			return Path.GetFileNameWithoutExtension(path);
		}

		public string GetFileDirectory(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));
			return Path.GetDirectoryName(path);
		}

		public ISuppressionFileWatcherService CreateWatcher(string path)
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
