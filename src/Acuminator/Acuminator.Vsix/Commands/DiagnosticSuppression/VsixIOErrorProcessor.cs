using System;
using System.Runtime.CompilerServices;
using System.Windows;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.IO;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// A VSIX I/O error processing for suppression command.
	/// </summary>
	public class VsixIOErrorProcessor : I_IOErrorProcessor
	{
		public void ProcessError(Exception exception, [CallerMemberName]string reportedFrom = null)
		{
			exception.ThrowOnNull(nameof(exception));

			string errorMsg = VSIXResource.FailedToLoadTheSuppressionFile + Environment.NewLine + Environment.NewLine +
							  string.Format(VSIXResource.FailedToLoadTheSuppressionFileDetails, Environment.NewLine + exception.Message);

			MessageBox.Show(errorMsg, AcuminatorVSPackage.PackageName, MessageBoxButton.OK, MessageBoxImage.Error);
			AcuminatorVSPackage.Instance.AcuminatorLogger?.LogException(exception, logOnlyFromAcuminatorAssemblies: false,
																		Logger.LogMode.Warning, reportedFrom);
		}
	}
}
