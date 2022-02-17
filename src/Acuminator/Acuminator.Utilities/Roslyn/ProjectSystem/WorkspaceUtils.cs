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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsProjectStatusInSolutionChanged(this WorkspaceChangeEventArgs changeEventArgs) =>
			changeEventArgs.CheckIfNull(nameof(changeEventArgs)).Kind switch
			{
				WorkspaceChangeKind.ProjectAdded => true,
				WorkspaceChangeKind.ProjectRemoved => true,
				_ => false
			};


		public static bool IsProjectMetadataChanged(this WorkspaceChangeEventArgs changeEventArgs)
		{
			changeEventArgs.ThrowOnNull(nameof(changeEventArgs));

			if ((changeEventArgs.Kind != WorkspaceChangeKind.ProjectChanged && 
				changeEventArgs.Kind != WorkspaceChangeKind.ProjectReloaded) || 
				changeEventArgs.ProjectId == null)
			{
				return false;
			}

			Project? oldProject = changeEventArgs.OldSolution.GetProject(changeEventArgs.ProjectId);
			Project? newProject = changeEventArgs.NewSolution.GetProject(changeEventArgs.ProjectId);

			if (oldProject == null || newProject == null)
				return false;

			// For simplicity and performance only the counts of metadata references are checked.
			// This does not cover a very rare case of one project metadata reference being replaced by another reference in a single operation.
			// For example it can be a manual edit of a project file. However, such operations are rare and are highly unlikely to affect Acuminator.
			// Thus for simplicity and performance reasons set equality is not checked for project metadata references
			return oldProject.MetadataReferences.Count == newProject.MetadataReferences.Count &&
				   oldProject.AllProjectReferences.Count == newProject.AllProjectReferences.Count;
		}
	}
}
