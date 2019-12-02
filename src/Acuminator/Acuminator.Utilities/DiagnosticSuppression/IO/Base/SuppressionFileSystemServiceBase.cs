using System;
using System.IO;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// A suppression file system service base class.
	/// </summary>
	public abstract class SuppressionFileSystemServiceBase : ISuppressionFileSystemService
	{
		public IIOErrorProcessor ErrorProcessor { get; }

		public SuppressionFileValidation FileValidation { get; }

		protected SuppressionFileSystemServiceBase(IIOErrorProcessor errorProcessor, SuppressionFileValidation customValidation)
		{
			ErrorProcessor = errorProcessor ?? new DefaultIOErrorProcessor();
			FileValidation = customValidation ?? new SuppressionFileValidation(ErrorProcessor);
		}

		public virtual XDocument Load(string path)
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

		public virtual bool Save(XDocument document, string path)
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

		public virtual string GetFileName(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			return Path.GetFileNameWithoutExtension(path);
		}

		public virtual string GetFileDirectory(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));
			return Path.GetDirectoryName(path);
		}

		public abstract ISuppressionFileWatcherService CreateWatcher(string path);

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
