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
			LogException(e);
		}

		public void LogException(FirstChanceExceptionEventArgs e)
		{		
			if (e.Exception.Source != AnalyzersDll && e.Exception.Source != UtilitiesDll && e.Exception.Source != VsixDll)
				return;

			IWpfTextView activeTextView = _package.GetWpfTextView();

			if (activeTextView == null)
				return;

			Document currentDocument = activeTextView.TextSnapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (currentDocument == null)
				return;

			string logMessage = CreateLogMessage(e, currentDocument);
			ActivityLog.TryLogError(PackageName, logMessage);
		}

		private string CreateLogMessage(FirstChanceExceptionEventArgs e, Document currentDocument)
		{
			StringBuilder messageBuilder = new StringBuilder();
			messageBuilder.AppendLine($"FILE PATH: {currentDocument.FilePath}")
						  .AppendLine($"MESSAGE: {e.Exception.Message}")
						  .AppendLine($"STACK TRACE: {e.Exception.StackTrace}")
						  .AppendLine($"TARGET SITE: {e.Exception.TargetSite}")
						  .AppendLine($"SOURCE: {e.Exception.Source}");

			return messageBuilder.ToString();
		}

		public void Dispose()
		{
			AppDomain.CurrentDomain.FirstChanceException -= Acuminator_FirstChanceException;
		}		
	}
}
