#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;

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

		private void Acuminator_FirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
		{
			LogException(e.Exception, logOnlyFromAcuminatorAssemblies: true, LogMode.Warning);
		}

		private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception exception)
			{
				LogMode logMode = e.IsTerminating 
					? LogMode.Error 
					: LogMode.Warning;

				LogException(exception, logOnlyFromAcuminatorAssemblies: false, logMode);
			}
		}

		private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
		{
			LogMode logMode = LogMode.Error;

			if (_swallowUnobservedTaskExceptions && !e.Observed)
			{
				e.SetObserved();
				logMode = LogMode.Warning;
			}

			foreach (Exception exception in e.Exception.Flatten().InnerExceptions)
			{
				LogException(exception, logOnlyFromAcuminatorAssemblies: false, logMode);
			} 
		}

		public static void LogMessage(string? message, LogMode logMode, [CallerMemberName] string? reportedFrom = null)
		{
			if (message.IsNullOrWhiteSpace() || logMode == LogMode.None)
				return;

			message = $"{AcuminatorVSPackage.PackageName.ToUpper()}: {message}";

			if (reportedFrom != null)
			{
				message += Environment.NewLine + $"REPORTED FROM: { reportedFrom }";
			}

			LogMessageToActivityLog(message, logMode);
		}

		public void LogException(Exception? exception, bool logOnlyFromAcuminatorAssemblies, LogMode logMode, 
								 [CallerMemberName]string? reportedFrom = null)
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

			IWpfTextView? activeTextView = GetActiveTextViewWithTimeout(timeoutSeconds: 20);

			if (activeTextView == null)
				return;

			Document? currentDocument = activeTextView.TextSnapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (currentDocument == null)
				return;

			string logMessage = CreateLogMessageFromException(exception, currentDocument.FilePath, logMode, reportedFrom, addErrorPrefix: true);
			LogMessageToActivityLog(logMessage, logMode);
		}

		public static void LogException(Exception? exception, LogMode logMode = LogMode.Error, bool addErrorPrefix = false,
										[CallerMemberName] string? reportedFrom = null)
		{
			if (exception == null || logMode == LogMode.None)
				return;

			string logMessage = CreateLogMessageFromException(exception, currentDocumentPath: null, logMode, reportedFrom, addErrorPrefix);
			LogMessageToActivityLog(logMessage, logMode);
		}

		private IWpfTextView? GetActiveTextViewWithTimeout(double timeoutSeconds)
		{
			try
			{
				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

				var joinableTask = ThreadHelper.JoinableTaskFactory.RunAsync(() => _package.GetWpfTextViewAsync());
				var activeTextView = joinableTask.Join(cts.Token);
				return activeTextView;
			}
			catch
			{
				return null;
			}
		}

		private static string CreateLogMessageFromException(Exception exception, string? currentDocumentPath, LogMode logMode, string? reportedFrom, 
															bool addErrorPrefix)
		{

			StringBuilder messageBuilder = new StringBuilder(capacity: 256);

			if (logMode == LogMode.Error && addErrorPrefix)
			{
				messageBuilder.AppendLine($"{AcuminatorVSPackage.PackageName.ToUpper()} OBSERVED AN UNHANDLED VISUAL STUDIO ERROR|");
			}

			messageBuilder = messageBuilder.AppendLine($"EXCEPTION TYPE: {exception.GetType().Name}");

			if (!currentDocumentPath.IsNullOrWhiteSpace())
				messageBuilder = messageBuilder.AppendLine($"|FILE PATH: {currentDocumentPath}");

			messageBuilder = messageBuilder
										.AppendLine($"|MESSAGE: {exception.Message}")
										.AppendLine($"|STACK TRACE: {exception.StackTrace}")
										.AppendLine($"|TARGET SITE: {exception.TargetSite}")
										.AppendLine($"|SOURCE: {exception.Source}");

			if (!reportedFrom.IsNullOrWhiteSpace())
				messageBuilder = messageBuilder.AppendLine($"|REPORTED FROM: {reportedFrom}");

			return messageBuilder.ToString();
		}

		private static void LogMessageToActivityLog(string logMessage, LogMode logMode)
		{
			switch (logMode)
			{
				case LogMode.Information:
					ActivityLog.TryLogInformation(AcuminatorVSPackage.PackageName, logMessage);
					break;
				case LogMode.Warning:
					ActivityLog.TryLogWarning(AcuminatorVSPackage.PackageName, logMessage);
					break;
				case LogMode.Error:
					ActivityLog.TryLogError(AcuminatorVSPackage.PackageName, logMessage);
					break;
			}
		}

		public void Dispose()
		{
			AppDomain.CurrentDomain.FirstChanceException -= Acuminator_FirstChanceException;
			AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
			TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
		}
	}
}
