#nullable enable
using System;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.Coloriser;

internal class DocumentWorkspaceChangedEventArgs(Workspace? oldWorkspace, Workspace? newWorkspace) : EventArgs()
{
	public Workspace? OldWorkspace { get; } = oldWorkspace;

	public Workspace? NewWorkspace { get; } = newWorkspace;
}
