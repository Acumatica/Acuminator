#nullable enable

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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
		private readonly AcuminatorVSPackage _package;
		private readonly bool _swallowUnobservedTaskExceptions;

		public AcuminatorLogger(AcuminatorVSPackage acuminatorPackage, bool swallowUnobservedTaskExceptions)
		{
			_package = acuminatorPackage.CheckIfNull();
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
			var innerExceptions = e.Exception.Flatten().InnerExceptions;

			if (innerExceptions.All(exception => exception is TimeoutException))    // Skip logging numerous VS timeout exceptions to decrease contention for UI thread
				return;

			var acuminatorExceptions = innerExceptions.Where(IsExceptionFromAcuminatorAssembly).ToList(innerExceptions.Count);

			if (acuminatorExceptions.Count == 0)	// Do not observe exceptions not from Acuminator
				return;

			LogMode logMode = LogMode.Error;

			if (_swallowUnobservedTaskExceptions && !e.Observed)
			{
				e.SetObserved();
				logMode = LogMode.Warning;
			}

			foreach (Exception exception in acuminatorExceptions)
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
			else if (logOnlyFromAcuminatorAssemblies && !IsExceptionFromAcuminatorAssembly(exception))
			{
				return;
			}

			IWpfTextView? activeTextView = ThreadHelper.CheckAccess() 
				? GetActiveTextViewWithTimeout(timeoutSeconds: 10)
				: null;

			Document? currentDocument;

			try
			{
				currentDocument = activeTextView?.TextSnapshot?.GetOpenDocumentInCurrentContextWithChanges();
			}
			catch
			{
				currentDocument = null;
			}

			string logMessage = CreateLogMessageFromException(exception, currentDocument?.FilePath, logMode, reportedFrom, addErrorPrefix: true);
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
				using var ctsByTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
				using var ctsByTimeoutAndPackage = 
					CancellationTokenSource.CreateLinkedTokenSource(ctsByTimeout.Token, AcuminatorVSPackage.Instance?.DisposalToken ?? default);

				var joinableTask = AcuminatorVSPackage.JTF.RunAsync(() => _package.GetWpfTextViewAsync());
				var activeTextView = joinableTask.Join(ctsByTimeoutAndPackage.Token);
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

		private static bool IsExceptionFromAcuminatorAssembly(Exception exception) =>
			exception.StackTrace?.Contains(AcuminatorVSPackage.PackageName) == true;
	}
}
