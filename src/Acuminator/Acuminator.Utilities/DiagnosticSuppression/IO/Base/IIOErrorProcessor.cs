using System;
using System.Runtime.CompilerServices;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// Interface for i/o error processing.
	/// </summary>
	public interface IIOErrorProcessor
	{
		void ProcessError(Exception exception, [CallerMemberName]string reportedFrom = null);
	}
}
