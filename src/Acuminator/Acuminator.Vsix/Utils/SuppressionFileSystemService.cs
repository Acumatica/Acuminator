using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using System.IO;
using System.Xml.Linq;

namespace Acuminator.Vsix.Utils
{
	internal class SuppressionFileSystemService : ISuppressionFileSystemService
	{
		public XDocument Load(string path)
		{
			path.ThrowOnNullOrWhiteSpace(nameof(path));

			return XDocument.Load(path);
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
	}
}
