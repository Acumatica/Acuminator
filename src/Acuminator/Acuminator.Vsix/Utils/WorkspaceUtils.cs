using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.Utilities
{
    /// <summary>
    /// A helper class with utility methods related to the Workspace.
    /// </summary>
    public static class WorkspaceUtils
	{       
		/// <summary>
		/// Get workspace indentation size.
		/// </summary>
		/// <param name="workspace">The workspace to act on.</param>
		/// <returns/>
		public static int GetWorkspaceIndentationSize(this Workspace workspace)
		{
			workspace.ThrowOnNull(nameof(workspace));
			return workspace.Options.GetOption(FormattingOptions.IndentationSize, LanguageNames.CSharp);
		}		

		public static bool IsActiveDocumentCleared(this WorkspaceChangeEventArgs changeEventArgs, Document oldDocument)
		{
			changeEventArgs.ThrowOnNull(nameof(changeEventArgs));

			switch (changeEventArgs.Kind)
			{
				case WorkspaceChangeKind.SolutionRemoved:					
				case WorkspaceChangeKind.SolutionCleared:		
				case WorkspaceChangeKind.SolutionReloaded:
					return oldDocument?.Project.Solution.Id == changeEventArgs.NewSolution.Id;
				case WorkspaceChangeKind.ProjectRemoved:
				case WorkspaceChangeKind.ProjectReloaded:
					return oldDocument?.Project.Id == changeEventArgs.ProjectId;
				case WorkspaceChangeKind.DocumentRemoved:
					return oldDocument?.Id == changeEventArgs.DocumentId;
				default:
					return false;
			}
		}

        public static bool IsActiveDocumentChanged(this WorkspaceChangeEventArgs changeEventArgs, Document oldDocument)
		{
			changeEventArgs.ThrowOnNull(nameof(changeEventArgs));

			if (changeEventArgs.Kind != WorkspaceChangeKind.DocumentChanged)
				return false;
			
			return HaveDocumentIdOrProjectIdChanged(changeEventArgs, oldDocument);
		}

		public static bool IsDocumentTextChanged(this WorkspaceChangeEventArgs changeEventArgs, Document oldDocument)
		{
			changeEventArgs.ThrowOnNull(nameof(changeEventArgs));

			if (changeEventArgs.Kind != WorkspaceChangeKind.DocumentChanged &&
				changeEventArgs.Kind != WorkspaceChangeKind.DocumentReloaded)
			{
				return false;
			}

			return !HaveDocumentIdOrProjectIdChanged(changeEventArgs, oldDocument);
		}

		private static bool HaveDocumentIdOrProjectIdChanged(WorkspaceChangeEventArgs changeEventArgs, Document oldDocument) =>
			oldDocument?.Id != changeEventArgs.DocumentId || oldDocument?.Project.Id != changeEventArgs.ProjectId;
	}
}
