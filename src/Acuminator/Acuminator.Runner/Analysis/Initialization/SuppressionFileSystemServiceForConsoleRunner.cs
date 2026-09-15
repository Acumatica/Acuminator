using Acuminator.Utilities.DiagnosticSuppression.IO;

namespace Acuminator.Runner.Analysis.Initialization
{
	internal class SuppressionFileSystemServiceForConsoleRunner : SuppressionFileSystemServiceBase
	{
		public SuppressionFileSystemServiceForConsoleRunner(IIOErrorProcessor errorProcessor) :
														base(errorProcessor, customValidation: null)
		{
		}

		public override ISuppressionFileWatcherService? CreateWatcher(string path) => null;
	}
}
