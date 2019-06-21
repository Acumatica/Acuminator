using System.Xml.Linq;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public interface ISuppressionFileSystemService
	{
		XDocument Load(string path);

		bool Save(XDocument document, string path);

		ISuppressionFileWatcherService CreateWatcher(string path);

		string GetFileName(string path);

		string GetFileDirectory(string path);
	}
}
