using System;
using System.IO;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public interface ISuppressionFileWatcherService : IDisposable
	{
		event FileSystemEventHandler Changed;
	}
}
