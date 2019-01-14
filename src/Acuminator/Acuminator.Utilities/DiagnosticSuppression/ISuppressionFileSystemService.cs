using System.Xml.Linq;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public interface ISuppressionFileSystemService
	{
		XDocument Load(string path);

		void Save(XDocument document, string path);

		ISuppressionFileWatcherService CreateWatcher(string path);

		string GetFileName(string path);
	}
}
