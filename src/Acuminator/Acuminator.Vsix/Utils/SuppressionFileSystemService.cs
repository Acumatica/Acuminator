using Acuminator.Utilities.DiagnosticSuppression;
using System.Xml.Linq;
using System;
using Acuminator.Utilities.Common;
using System.IO;

namespace Acuminator.Vsix.Utils
{
	internal class SuppressionFileSystemService : ISuppressionFileSystemService
	{
		public XDocument Load(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			return XDocument.Load(path);
		}

		public void Save(XDocument document, string path)
		{
			document.ThrowOnNull(nameof(document));

			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			document.Save(path);
		}

		public string GetFileName(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			return Path.GetFileNameWithoutExtension(path);
		}
	}
}
