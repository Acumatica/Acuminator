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
			LogException(e.Exception, logOnlyFromAcuminatorAssemblies: true, LogMode.Warning);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception exception)
			{
				LogMode logMode = e.IsTerminating 
					? LogMode.Error 
					: LogMode.Warning;

				LogException(exception, logOnlyFromAcuminatorAssemblies: false, logMode);
			}
		}

		private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			LogMode logMode = LogMode.Error;

			if (_swallowUnobservedTaskExceptions && !e.Observed)
			{
				e.SetObserved();
				logMode = LogMode.Warning;
			}

			e.Exception.Flatten().InnerExceptions
								 .ForEach(exception => LogException(exception, logOnlyFromAcuminatorAssemblies: false, logMode));		
		}


		public void LogException(Exception exception, bool logOnlyFromAcuminatorAssemblies, LogMode logMode)
		{
			if (exception == null || logMode == LogMode.None)
			{
				return;
			}
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

			string logMessage = CreateLogMessageFromException(exception, currentDocument, logMode);

			switch (logMode)
			{
				case LogMode.Information:
					ActivityLog.TryLogInformation(PackageName, logMessage);
					break;
				case LogMode.Warning:
					ActivityLog.TryLogWarning(PackageName, logMessage);
					break;
				case LogMode.Error:
					ActivityLog.TryLogError(PackageName, logMessage);
					break;
			}		
		}

		private string CreateLogMessageFromException(Exception exception, Document currentDocument, LogMode logMode)
		{

			StringBuilder messageBuilder = new StringBuilder(capacity: 256);

			if (logMode == LogMode.Error)
			{
				messageBuilder.AppendLine($"{PackageName.ToUpper()} CAUSED CRITICAL ERROR|");
			}

			messageBuilder.AppendLine($"EXCEPTION TYPE: {exception.GetType().Name}")
						  .AppendLine($"|FILE PATH: {currentDocument.FilePath}")
						  .AppendLine($"|MESSAGE: {exception.Message}")
						  .AppendLine($"|STACK TRACE: {exception.StackTrace}")
						  .AppendLine($"|TARGET SITE: {exception.TargetSite}")
						  .AppendLine($"|SOURCE: {exception.Source}");

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
