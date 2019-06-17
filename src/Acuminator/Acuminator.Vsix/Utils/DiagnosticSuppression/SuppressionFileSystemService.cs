using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using System.IO;
using System.Security;
using System.Xml.Linq;

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

		public void Save(XDocument document, string path)
		{
			document.ThrowOnNull(nameof(document));
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			document.Save(path);
		}

		public string GetFileName(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			return Path.GetFileNameWithoutExtension(path);
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
