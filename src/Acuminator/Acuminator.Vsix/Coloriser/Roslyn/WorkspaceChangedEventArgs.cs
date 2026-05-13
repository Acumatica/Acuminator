#nullable enable
using System;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.Coloriser;

public class WorkspaceChangedEventArgs(Workspace? oldWorkspace, Workspace? newWorkspace) : EventArgs()
{
	public Workspace? OldWorkspace { get; } = oldWorkspace;

	public Workspace? NewWorkspace { get; } = newWorkspace;
}
