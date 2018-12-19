using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;



namespace Acuminator.Vsix.Utilities
{
    /// <summary>
    /// A helper class with utility methods related to the workspace changed event.
    /// </summary>
    public static class WorkspaceChangedUtils
	{       
        public static bool IsActiveDocumentChanged(this WorkspaceChangeEventArgs changeEventArgs, Document oldDocument)
		{
			oldDocument.ThrowOnNull(nameof(oldDocument));
			changeEventArgs.ThrowOnNull(nameof(changeEventArgs));

			if (changeEventArgs.Kind != WorkspaceChangeKind.DocumentChanged)
				return false;
			
			return HaveDocumentIdOrProjectIdChanged(changeEventArgs, oldDocument);
		}

		public static bool IsDocumentTextChanged(this WorkspaceChangeEventArgs changeEventArgs, Document oldDocument)
		{
			oldDocument.ThrowOnNull(nameof(oldDocument));
			changeEventArgs.ThrowOnNull(nameof(changeEventArgs));

			if (changeEventArgs.Kind != WorkspaceChangeKind.DocumentChanged)
				return false;

			return !HaveDocumentIdOrProjectIdChanged(changeEventArgs, oldDocument);
		}

		private static bool HaveDocumentIdOrProjectIdChanged(WorkspaceChangeEventArgs changeEventArgs, Document oldDocument) =>
			oldDocument.Id != changeEventArgs.DocumentId || oldDocument.Project.Id != changeEventArgs.ProjectId;
	}
}
