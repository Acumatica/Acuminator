using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.DiagnosticSuppression.IO
{
	/// <summary>
	/// Default implementation for Diagnostic Suppression IO error processor.
	/// </summary>
	public class DefaultIOErrorProcessor : IIOErrorProcessor
	{
		public void ProcessError(Exception exception, [CallerMemberName]string reportedFrom = null)
		{
			exception.ThrowOnNull(nameof(exception));
			string errorMsg = Resources.FailedToLoadTheSuppressionFile + Environment.NewLine + Environment.NewLine +
							  string.Format(Resources.FailedToLoadTheSuppressionFileDetails, Environment.NewLine + exception.Message);

			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMsg}");
		}
	}
}
