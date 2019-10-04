using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	internal class SuppressionFileSystemService
	{
		public event EventHandler<IOErrorEventArgs> OnLoadError;
		public event EventHandler<IOErrorEventArgs> OnSaveError;

		public XDocument Load(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			try
			{
				return XDocument.Load(path);
			}
			catch (Exception exception) when (FilterException(exception))
			{
				ProcessError(exception);
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
			catch (Exception exception) when (FilterException(exception))
			{
				ProcessError(exception);
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

		public FileSystemWatcher CreateWatcher(string path)
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

		private bool FilterException(Exception exception)
		{
			switch (exception)
			{
				case XmlException _:
				case SecurityException _:
				case IOException _:
					return true;
				default:
					return false;
			}
		}

		private void ProcessError(Exception exception, [CallerMemberName]string reportedFrom = null)
		{
			string errorMsg = VSIXResource.FailedToLoadTheSuppressionFile + Environment.NewLine + Environment.NewLine +
							  string.Format(VSIXResource.FailedToLoadTheSuppressionFileDetails, Environment.NewLine + exception.Message);
			MessageBox.Show(errorMsg, AcuminatorVSPackage.PackageName, MessageBoxButton.OK, MessageBoxImage.Error);
			AcuminatorVSPackage.Instance.AcuminatorLogger?.LogException(exception, logOnlyFromAcuminatorAssemblies: false,
																		Logger.LogMode.Warning, reportedFrom);
		}
	}
}
