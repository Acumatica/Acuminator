#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Telemetry;
using Microsoft.Internal.VisualStudio.Shell;

namespace Acuminator.Vsix.Utilities;

/// <summary>
/// The threading and task related utilities that use VS threading mechanisms.
/// </summary>
public static class VsTasksUtils
{
	/// <inheritdoc cref="FileAndForget(System.Threading.Tasks.Task, string?, CancellationToken, string?, bool, Func{Exception, bool}?)"/>
	/// <param name="joinableTask">The <see cref="JoinableTask"/> to act on.</param>
	public static void FileAndForget(this JoinableTask joinableTask, string? faultEventName, CancellationToken cancellation, 
									 string? faultDescription = null, bool logCancellations = false, Func<Exception, bool>? fileOnlyIf = null) =>
		FileAndForget(joinableTask.CheckIfNull().Task, faultEventName, cancellation, faultDescription, logCancellations, fileOnlyIf);

	/// <summary>
	/// A <see cref="System.Threading.Tasks.Task"/> extension method that file and forget.
	/// </summary>
	/// <remarks>
	/// This code is written by example from <see cref="Microsoft.VisualStudio.Shell.VsTaskLibraryHelper"/>.FileAndForget method<br/>
	/// which provides an example of how to handle fire-and-forget async action inside void-returning event handlers<br/>
	/// with the use of JTF and VS telemetry mechanisms.<br/>
	/// <br/>
	/// The main reason of having a separate method instead of using the <see cref="Microsoft.VisualStudio.Shell.VsTaskLibraryHelper"/>.FileAndForget method is to<br/>
	/// be able to use <see cref="JoinableTaskFactory"/> from the <see cref="AcuminatorVSPackage"/> class instead of the default one from <see cref="Microsoft.VisualStudio.Shell.ThreadHelper"/>.
	/// </remarks>
	/// <param name="task">The task to act on.</param>
	/// <param name="faultEventName">Name of the fault event. Use the name of the component for this with the following convention:<br/>
	/// <c>"vs/{AcuminatorVSPackage.PackageName}/{componentName}/{methodName}"</c>.</param>
	/// <param name="cancellation">A token that allows processing to be cancelled.</param>
	/// <param name="faultDescription">(Optional) Information describing the fault.</param>
	/// <param name="logCancellations">(Optional) True to log cancellation exceptions. False by default.</param>
	/// <param name="fileOnlyIf">(Optional) The optional condition on exceptions to be logged. Takes precedence over the <paramref name="logCancellations"/> flag.</param>
	public static void FileAndForget(this System.Threading.Tasks.Task task, string? faultEventName, CancellationToken cancellation,
									 string? faultDescription = null, bool logCancellations = false, Func<Exception, bool>? fileOnlyIf = null)
	{
		task.ThrowOnNull();
		JoinableTask joinableTask = AcuminatorVSPackage.JTF.RunAsync(async delegate
		{
			try
			{
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks - This is already in JTF.RunAsync method, so we can safely await the task here.
				await task.ConfigureAwait(continueOnCapturedContext: false);
#pragma warning restore VSTHRD003
			}
			catch (Exception ex) when (FilterExceptions(ex, fileOnlyIf, logCancellations))
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				FaultEvent telemetryEvent = new FaultEvent(faultEventName, faultDescription, ex)
				{
					IsIncludedInWatsonSample = false
				};

				TelemetryHelper.DataModelTelemetrySession?.PostEvent(telemetryEvent);
				faultDescription = faultDescription.NullIfWhiteSpace()?.Trim();
				string text = faultDescription != null 
					? faultDescription + Environment.NewLine 
					: string.Empty;
				text += ex;

				ActivityLog.TryLogError(faultEventName, text);
			}
		});
	}

	private static bool FilterExceptions(Exception exception, Func<Exception, bool>? fileOnlyIf, bool logCancellations)
	{
		if (fileOnlyIf?.Invoke(exception) == true)
			return true;
		else if (exception is OperationCanceledException)
			return logCancellations;
		else
			return true;
	}
}