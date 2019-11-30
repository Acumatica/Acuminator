using System;
using System.IO;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	public class SuppressionFileSystemService : ISuppressionFileSystemService
	{
		public IIOErrorProcessor ErrorProcessor { get; }

		public SuppressionFileValidation FileValidation { get; }

		public SuppressionFileSystemService() : this(null, null)
		{
		}

		public SuppressionFileSystemService(IIOErrorProcessor errorProcessor) : this(errorProcessor, null)
		{
		}

		public SuppressionFileSystemService(IIOErrorProcessor errorProcessor, SuppressionFileValidation customValidation)
		{
			ErrorProcessor = errorProcessor ?? new DefaultIOErrorProcessor();
			FileValidation = customValidation ?? new SuppressionFileValidation(ErrorProcessor);
		}

		public XDocument Load(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			try
			{
				if (!File.Exists(path))
					return null;

				var document = XDocument.Load(path);

				if (document == null)
					return null;	
				else if (FileValidation != null && !FileValidation.ValidateSuppressionFile(document))
					return null;

				return document;
			}
			catch (Exception exception) when (FilterException(exception))
			{
				ErrorProcessor.ProcessError(exception);
			}

			return null;
		}

		public bool Save(XDocument document, string path)
		{
			document.ThrowOnNull(nameof(document));
			path.ThrowOnNullOrWhiteSpace(nameof(path));
			
			try
			{
				if (FileValidation != null && !FileValidation.ValidateSuppressionFile(document))
					return false;

				document.Save(path);
			}
			catch (Exception exception) when (FilterException(exception))
			{
				ErrorProcessor.ProcessError(exception);
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
	}
}
