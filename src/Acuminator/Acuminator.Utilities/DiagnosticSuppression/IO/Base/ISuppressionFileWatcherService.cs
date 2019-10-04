using System;
using System.IO;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	public interface ISuppressionFileWatcherService : IDisposable
	{
		event FileSystemEventHandler Changed;
	}
}
