using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using FirstChanceExceptionEventArgs = System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs;


namespace Acuminator.Vsix.Logger
{
	/// <summary>
	/// An acuminator logger used to log unhandled exceptions.
	/// </summary>
	internal class AcuminatorLogger : IDisposable
	{
		private readonly string AnalyzersDll = typeof(PXDiagnosticAnalyzer).Assembly.GetName().Name;
		private readonly string UtilitiesDll = typeof(CodeAnalysisSettings).Assembly.GetName().Name;
		private readonly string VsixDll = typeof(AcuminatorLogger).Assembly.GetName().Name;

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
			ActivityLog.TryLogError(AcuminatorVSPackage.PackageName, logMessage);
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
