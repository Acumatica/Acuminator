using System;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public interface ISuppressionFileWatcherService
	{
		event Action<object, SuppressionFileEventArgs> Changed;
	}
}
