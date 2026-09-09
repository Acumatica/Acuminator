#nullable enable

using System;
using System.Linq;

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
	/// <summary>
	/// A <see cref="Func{System.Threading.Tasks.Task}"/> extension method that runs async method <paramref name="asyncMethod"/> 
	/// in a correct context of JTF, files exceptions and forgets.
	/// </summary>
	/// <remarks>
	/// The code is based on <see cref="Microsoft.VisualStudio.Shell.VsTaskLibraryHelper"/>.FileAndForget method<br/>
	/// which provides an example of how to handle fire-and-forget async action inside void-returning event handlers<br/>
	/// with the use of JTF and VS telemetry mechanisms.<br/>
	/// <br/>
	/// The reason of having a separate method instead of using the <see cref="Microsoft.VisualStudio.Shell.VsTaskLibraryHelper"/>.FileAndForget method is to<br/>
	/// be able to use <see cref="JoinableTaskFactory"/> from the <see cref="AcuminatorVSPackage"/> class instead of the default one from <see cref="ThreadHelper"/> to run the async method.<br/>
	/// This code also supports skipping of <see cref="OperationCanceledException"/>s.
	/// </remarks>
	/// <param name="asyncMethod">The async method to act on.</param>
	/// <param name="faultEventName">Name of the fault event. Use the name of the component for this with the following convention:<br/>
	/// <c>"vs/{AcuminatorVSPackage.PackageName}/{componentName}/{methodName}"</c>.</param>
	/// <param name="faultDescription">(Optional) Information describing the fault.</param>
	/// <param name="logCancellations">(Optional) True to log cancellation exceptions. False by default.</param>
	/// <param name="fileOnlyIf">(Optional) The optional condition on exceptions to be logged. Takes precedence over the <paramref name="logCancellations"/> flag.</param>
	public static void FileAndForgetAcuminatorTask(this Func<System.Threading.Tasks.Task>? asyncMethod, string? faultEventName, string? faultDescription = null, 
												   bool logCancellations = false, Func<Exception, bool>? fileOnlyIf = null)
	{
		asyncMethod.ThrowOnNull();
		JoinableTask joinableTask = AcuminatorVSPackage.JTF.RunAsync(async delegate
		{
			try
			{
				await asyncMethod();
			}
			catch (Exception ex)
			{
				if (!ShouldLogException(ex, fileOnlyIf, logCancellations))
					return;

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

	private static bool ShouldLogException(Exception exception, Func<Exception, bool>? fileOnlyIf, bool logCancellations)
	{
		if (fileOnlyIf != null)
			return fileOnlyIf(exception);
		else if (exception is OperationCanceledException)
			return logCancellations;
		else
			return true;
	}
}