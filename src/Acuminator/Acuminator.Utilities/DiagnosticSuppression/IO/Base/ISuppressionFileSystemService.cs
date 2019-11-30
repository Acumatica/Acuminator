using System.Xml.Linq;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	public interface ISuppressionFileSystemService
	{
		IIOErrorProcessor ErrorProcessor { get; }

		SuppressionFileValidation FileValidation { get; }

		XDocument Load(string path);

		bool Save(XDocument document, string path);

		ISuppressionFileWatcherService CreateWatcher(string path);

		string GetFileName(string path);

		string GetFileDirectory(string path);
	}
}
