#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;


namespace Acuminator.Utilities.Roslyn.ProjectSystem
{
    /// <summary>
    /// A helper class with utility methods related to the Workspace.
    /// </summary>
    public static class WorkspaceUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SuppressionManagerInitInfo> GetSuppressionInfo(this Solution solution, bool generateSuppressionBase)
		{
			var suppressionFiles = solution.GetAllAdditionalDocuments()
										   .Where(additionalDoc => SuppressionFile.IsSuppressionFile(additionalDoc.FilePath));
			return suppressionFiles.Select(file => new SuppressionManagerInitInfo(file.FilePath, generateSuppressionBase));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TextDocument> GetAllAdditionalDocuments(this Solution solution) =>
			solution.CheckIfNull(nameof(solution)).Projects.SelectMany(p => p.AdditionalDocuments);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TextDocument> GetSuppressionFiles(this Project project) =>
			project.CheckIfNull(nameof(project)).AdditionalDocuments
												.Where(additionalDoc => SuppressionFile.IsSuppressionFile(additionalDoc.FilePath));

		/// <summary>
		/// Get workspace indentation size.
		/// </summary>
		/// <param name="workspace">The workspace to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetWorkspaceIndentationSize(this Workspace workspace)
		{
			workspace.ThrowOnNull(nameof(workspace));
			return workspace.Options.GetOption(FormattingOptions.IndentationSize, LanguageNames.CSharp);
		}		

		public static bool IsActiveDocumentCleared(this WorkspaceChangeEventArgs changeEventArgs, Document? oldDocument) =>
			changeEventArgs.CheckIfNull(nameof(changeEventArgs)).Kind switch
			{
				var kind when kind == WorkspaceChangeKind.SolutionRemoved ||
							  kind == WorkspaceChangeKind.SolutionCleared ||
							  kind == WorkspaceChangeKind.SolutionReloaded => oldDocument?.Project.Solution.Id == changeEventArgs.NewSolution.Id,

				var kind when kind == WorkspaceChangeKind.ProjectRemoved ||
							  kind == WorkspaceChangeKind.ProjectReloaded => oldDocument?.Project.Id == changeEventArgs.ProjectId,

				WorkspaceChangeKind.DocumentRemoved => oldDocument?.Id == changeEventArgs.DocumentId,
				_ => false
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsActiveDocumentChanged(this WorkspaceChangeEventArgs changeEventArgs, Document? oldDocument)
		{
			changeEventArgs.ThrowOnNull(nameof(changeEventArgs));

			if (changeEventArgs.Kind != WorkspaceChangeKind.DocumentChanged)
				return false;
			
			return HaveDocumentIdOrProjectIdChanged(changeEventArgs, oldDocument);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDocumentTextChanged(this WorkspaceChangeEventArgs changeEventArgs, Document? oldDocument)
		{
			changeEventArgs.ThrowOnNull(nameof(changeEventArgs));

			if (changeEventArgs.Kind != WorkspaceChangeKind.DocumentChanged &&
				changeEventArgs.Kind != WorkspaceChangeKind.DocumentReloaded)
			{
				return false;
			}

			return !HaveDocumentIdOrProjectIdChanged(changeEventArgs, oldDocument);
		}

		private static bool HaveDocumentIdOrProjectIdChanged(WorkspaceChangeEventArgs changeEventArgs, Document? oldDocument) =>
			oldDocument?.Id != changeEventArgs.DocumentId || oldDocument?.Project.Id != changeEventArgs.ProjectId;
	}
}
