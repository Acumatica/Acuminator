using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities;

using FirstChanceExceptionEventArgs = System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs;


namespace Acuminator.Vsix.Logger
{
	/// <summary>
	/// An acuminator logger used to log unhandled exceptions.
	/// </summary>
	internal class AcuminatorLogger : IDisposable
	{
		private const string PackageName = "Acuminator";

		private const string AnalyzersDll = "Acuminator.Analyzers";
		private const string UtilitiesDll = "Acuminator.Utils";
		private const string VsixDll = "Acuminator.Vsix";

		private readonly AcuminatorVSPackage _package;

		public AcuminatorLogger(AcuminatorVSPackage acuminatorPackage)
		{
			acuminatorPackage.ThrowOnNull(nameof(acuminatorPackage));
			_package = acuminatorPackage;
			AppDomain.CurrentDomain.FirstChanceException += Acuminator_FirstChanceException;
		}

		private void Acuminator_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
		{
			LogException(e.Exception);
		}

		public void LogException(Exception exception)
		{		
			if (exception == null || (exception.Source != AnalyzersDll && exception.Source != UtilitiesDll && exception.Source != VsixDll))
				return;

			IWpfTextView activeTextView = _package.GetWpfTextView();

			if (activeTextView == null)
				return;

			Document currentDocument = activeTextView.TextSnapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (currentDocument == null)
				return;

			string logMessage = CreateLogMessageFromException(exception, currentDocument);
			ActivityLog.TryLogError(PackageName, logMessage);
		}

		private string CreateLogMessageFromException(Exception exception, Document currentDocument)
		{
			StringBuilder messageBuilder = new StringBuilder(capacity: 256);
			messageBuilder.AppendLine($"FILE PATH: {currentDocument.FilePath}")
						  .AppendLine($"MESSAGE: {exception.Message}")
						  .AppendLine($"STACK TRACE: {exception.StackTrace}")
						  .AppendLine($"TARGET SITE: {exception.TargetSite}")
						  .AppendLine($"SOURCE: {exception.Source}");

			return messageBuilder.ToString();
		}

		public void Dispose()
		{
			AppDomain.CurrentDomain.FirstChanceException -= Acuminator_FirstChanceException;
		}		
	}
}
