#nullable enable
using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Logger;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace Acuminator.Vsix.Coloriser;

/// <summary>
/// A Roslyn workspace provider that tracks workspace changes.
/// </summary>
internal class RoslynWorkspaceProvider : IDisposable
{
	private readonly WorkspaceRegistration _workspaceRegistration;

	/// <summary>
	/// Gets the workspace subscription locker.
	/// </summary>
	/// <value>
	/// The workspace subscription locker.
	/// </value>
	/// <remarks>
	/// This locker has to be externally available because external subscribers like Roslyn colorizer subscribe on workspace changes and have unavoidable race condition<br/>
	/// that has to use this locker to synchronize with change of the Roslyn workspace associated with a VS text buffer.
	/// </remarks>
	public object WorkspaceSubscriptionLocker { get; } = new object();

	private Workspace? _workspace;

	public Workspace? Workspace
	{ 
		get 
		{
			lock (WorkspaceSubscriptionLocker)
			{
				return _workspace;
			}
		}
	}

	public event EventHandler<DocumentWorkspaceChangedEventArgs>? WorkspaceChanged;

	public RoslynWorkspaceProvider(ITextBuffer buffer)
	{
		SourceTextContainer sourceTextContainer = buffer.CheckIfNull().AsTextContainer();
		_workspaceRegistration = Workspace.GetWorkspaceRegistration(sourceTextContainer);
		_workspaceRegistration.WorkspaceChanged += OnWorkspaceChanged;

		_workspace = GetWorkspaceThatSupportsColoring(_workspaceRegistration);
	}

	private void OnWorkspaceChanged(object sender, EventArgs e)
	{
		Workspace? newWorkspace = GetWorkspaceThatSupportsColoring(_workspaceRegistration);
		Workspace? oldWorkspace;

		lock (WorkspaceSubscriptionLocker)
		{
			oldWorkspace = _workspace;

			// Swallow event if the workspace didn't really change or if it changed to another workspace that doesn't support coloring
			if (ReferenceEquals(oldWorkspace, newWorkspace))
				return;

			_workspace = newWorkspace;
		}

		DocumentWorkspaceChangedEventArgs eventArgs = new(oldWorkspace, newWorkspace);
		InvokeWorkspaceChangedEventSafely(eventArgs);
	}

	public void Dispose()
	{
		_workspaceRegistration.WorkspaceChanged -= OnWorkspaceChanged;
		WorkspaceChanged = null;

		lock (WorkspaceSubscriptionLocker)
		{
			// Clear workspace reference to prevent any external usage after disposal
			_workspace = null;
		}
	}

	private void InvokeWorkspaceChangedEventSafely(DocumentWorkspaceChangedEventArgs eventArgs)
	{
		try
		{
			WorkspaceChanged?.Invoke(this, eventArgs);
		}
		catch (Exception exception)
		{
			AcuminatorVSPackage.Instance?.AcuminatorLogger?.LogException(exception, logOnlyFromAcuminatorAssemblies: false,
																		 LogMode.Warning);
		}
	}

	private static Workspace? GetWorkspaceThatSupportsColoring(WorkspaceRegistration workspaceRegistration)
	{
		var workspace = workspaceRegistration.Workspace;

		if (workspace == null)
			return null;

		const string previewWorkspaceKind = "MiscellaneousFiles";
		return previewWorkspaceKind.Equals(workspace.Kind, StringComparison.OrdinalIgnoreCase)
			? null
			: workspace;
	}
}
