using System;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public interface ISuppressionFileWatcherService : IDisposable
	{
		event Action<object, SuppressionFileEventArgs> Changed;
	}
}
