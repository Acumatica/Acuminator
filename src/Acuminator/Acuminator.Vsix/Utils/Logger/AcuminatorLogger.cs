using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
		public const string PackageName = "Acuminator";

		private readonly string AnalyzersDll = typeof(PXDiagnosticAnalyzer).Assembly.GetName().Name;
		private readonly string UtilitiesDll = typeof(CodeAnalysisSettings).Assembly.GetName().Name;
		private readonly string VsixDll = typeof(AcuminatorLogger).Assembly.GetName().Name;

		private readonly AcuminatorVSPackage _package;
		private readonly bool _swallowUnobservedTaskExceptions;

		public AcuminatorLogger(AcuminatorVSPackage acuminatorPackage, bool swallowUnobservedTaskExceptions)
		{
			acuminatorPackage.ThrowOnNull(nameof(acuminatorPackage));
			_package = acuminatorPackage;
			_swallowUnobservedTaskExceptions = swallowUnobservedTaskExceptions;

			AppDomain.CurrentDomain.FirstChanceException += Acuminator_FirstChanceException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		}

		private void Acuminator_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
		{
			LogException(e.Exception, logOnlyFromAcuminatorAssemblies: true, isCriticalError: false);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception exception)
			{
				LogException(exception, logOnlyFromAcuminatorAssemblies: false, isCriticalError: e.IsTerminating);
			}
		}

		private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			bool isCriticalError = true;

			if (_swallowUnobservedTaskExceptions && !e.Observed)
			{
				e.SetObserved();
				isCriticalError = false;
			}

			e.Exception.Flatten().InnerExceptions
								 .ForEach(exception => LogException(exception, logOnlyFromAcuminatorAssemblies: false, isCriticalError));		
		}


		public void LogException(Exception exception, bool logOnlyFromAcuminatorAssemblies, bool isCriticalError)
		{		
			if (exception == null)
				return;
			else if (logOnlyFromAcuminatorAssemblies && 
					 exception.Source != AnalyzersDll && exception.Source != UtilitiesDll && exception.Source != VsixDll)
			{
				return;
			}

			IWpfTextView activeTextView = _package.GetWpfTextView();

			if (activeTextView == null)
				return;

			Document currentDocument = activeTextView.TextSnapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (currentDocument == null)
				return;

			string logMessage = CreateLogMessageFromException(exception, currentDocument, isCriticalError);
			ActivityLog.TryLogError(PackageName, logMessage);
		}

		private string CreateLogMessageFromException(Exception exception, Document currentDocument, bool isCriticalError)
		{

			StringBuilder messageBuilder = new StringBuilder(capacity: 256);

			if (isCriticalError)
			{
				messageBuilder.AppendLine($"{PackageName} CAUSED CRITICAL ERROR");
			}

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
			AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
			TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
		}		
	}
}
