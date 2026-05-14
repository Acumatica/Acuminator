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

	private readonly object _locker = new object();
	private Workspace? _workspace;

	public Workspace? Workspace
	{ 
		get 
		{
			lock (_locker)
			{
				return _workspace;
			}
		}
		private set 
		{
			if (!ReferenceEquals(_workspace, value))
			{
				lock (_locker)
				{
					if (!ReferenceEquals(_workspace, value))
					{
						_workspace = value;
					}
				}
			}
		}
	}

	public event EventHandler<DocumentWorkspaceChangedEventArgs>? WorkspaceChanged;

	public RoslynWorkspaceProvider(ITextBuffer buffer)
	{
		SourceTextContainer sourceTextContainer = buffer.CheckIfNull().AsTextContainer();
		_workspaceRegistration = Workspace.GetWorkspaceRegistration(sourceTextContainer);
		_workspaceRegistration.WorkspaceChanged += OnWorkspaceChanged;

		Workspace = GetWorkspaceThatSupportsColoring(_workspaceRegistration);
	}

	private void OnWorkspaceChanged(object sender, EventArgs e)
	{
		Workspace? newWorkspace = GetWorkspaceThatSupportsColoring(_workspaceRegistration);
		Workspace? oldWorkspace;

		lock (_locker)
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

		// Clear workspace reference to prevent any external usage after disposal
		var oldWorkspace = Workspace;
		Workspace = null;

		// Let subscribers know that the workspace is no longer available due to disposal and unsubscribe from events to prevent memory leaks
		DocumentWorkspaceChangedEventArgs disposalEventArgs = new(oldWorkspace, null);
		InvokeWorkspaceChangedEventSafely(disposalEventArgs);

		// Clear event subscribers
		WorkspaceChanged = null;
	}

	private void InvokeWorkspaceChangedEventSafely(DocumentWorkspaceChangedEventArgs eventArgs)
	{
		try
		{
			WorkspaceChanged?.Invoke(this, eventArgs);
		}
		catch (Exception exception)
		{
			AcuminatorVSPackage.Instance.AcuminatorLogger.LogException(exception, logOnlyFromAcuminatorAssemblies: false,
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
