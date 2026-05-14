#nullable enable
using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace Acuminator.Vsix.Coloriser;

/// <summary>
/// A Roslyn workspace provider that tracks workspace changes.
/// </summary>
internal class RoslynWorkspaceProvider : IDisposable
{
	private readonly ITextBuffer _buffer;
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
		_buffer = buffer.CheckIfNull();

		SourceTextContainer sourceTextContainer = _buffer.AsTextContainer();
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
		WorkspaceChanged?.Invoke(this, eventArgs);
	}

	public void Dispose()
	{
		WorkspaceChanged = null;
		_workspaceRegistration.WorkspaceChanged -= OnWorkspaceChanged;
		Workspace = null;
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
