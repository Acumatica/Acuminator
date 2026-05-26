using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Microsoft.CodeAnalysis.Diagnostics;


namespace Acuminator.Utilities.Roslyn.ProjectSystem
{
    /// <summary>
    /// A helper class with utility methods related to the Workspace.
    /// </summary>
    public static class WorkspaceUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<GlobalSuppressionFileInitInfo> GetSuppressionInfo(this Solution solution, AcuminatorWorkMode workMode)
		{
			var suppressionFiles = solution.GetAllAdditionalDocuments()
										   .Where(additionalDoc => !additionalDoc.FilePath.IsNullOrWhiteSpace() &&
																	additionalDoc.FilePath.IsSuppressionFile(checkFileExists: false));
			return suppressionFiles.Where(file => !file.FilePath.IsNullOrWhiteSpace())
								   .Select(file => new GlobalSuppressionFileInitInfo(file.FilePath!, workMode));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TextDocument> GetAllAdditionalDocuments(this Solution solution) =>
			solution.CheckIfNull().Projects.SelectMany(p => p.AdditionalDocuments);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TextDocument> GetSuppressionFiles(this Project project) =>
			project.CheckIfNull().AdditionalDocuments
								 .Where(additionalDoc => !additionalDoc.FilePath.IsNullOrWhiteSpace() &&
														  additionalDoc.FilePath.IsSuppressionFile(checkFileExists: false));

		/// <summary>
		/// Get workspace indentation size.
		/// </summary>
		/// <param name="workspace">The workspace to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetWorkspaceIndentationSize(this Workspace workspace)
		{
			workspace.ThrowOnNull();
			return workspace.Options.GetOption(FormattingOptions.IndentationSize, LanguageNames.CSharp);
		}		

		public static bool IsActiveDocumentCleared(this WorkspaceChangeEventArgs changeEventArgs, Document? oldDocument) =>
			changeEventArgs.CheckIfNull().Kind switch
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
			changeEventArgs.ThrowOnNull();

			if (changeEventArgs.Kind != WorkspaceChangeKind.DocumentChanged)
				return false;
			
			return HaveDocumentIdOrProjectIdChanged(changeEventArgs, oldDocument);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDocumentTextChanged(this WorkspaceChangeEventArgs changeEventArgs, Document? oldDocument)
		{
			changeEventArgs.ThrowOnNull();

			if (changeEventArgs.Kind != WorkspaceChangeKind.DocumentChanged &&
				changeEventArgs.Kind != WorkspaceChangeKind.DocumentReloaded)
			{
				return false;
			}

			return !HaveDocumentIdOrProjectIdChanged(changeEventArgs, oldDocument);
		}

		private static bool HaveDocumentIdOrProjectIdChanged(WorkspaceChangeEventArgs changeEventArgs, Document? oldDocument) =>
			oldDocument?.Id != changeEventArgs.DocumentId || oldDocument?.Project.Id != changeEventArgs.ProjectId;

		public static bool IsFullyLoadedProject(this Project project) =>
			project.CheckIfNull().FilePath.NullIfWhiteSpace() != null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsProjectStatusInSolutionChanged(this WorkspaceChangeEventArgs changeEventArgs) =>
			changeEventArgs.CheckIfNull().Kind switch
			{
				WorkspaceChangeKind.ProjectAdded => true,
				WorkspaceChangeKind.ProjectRemoved => true,
				_ => false
			};


		public static bool IsProjectMetadataChanged(this WorkspaceChangeEventArgs changeEventArgs)
		{
			changeEventArgs.ThrowOnNull();

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

			// For simplicity and performance only total counts of metadata references are checked.
			// This does not cover a very rare case of one project metadata reference being replaced by another reference in a single operation.
			// For example it can be a manual edit of a project file. However, such operations are rare and are highly unlikely to affect Acuminator.
			// Thus for simplicity and performance reasons set equality is not checked for project metadata references
			return oldProject.MetadataReferences.Count != newProject.MetadataReferences.Count ||
				   oldProject.AllProjectReferences.Count != newProject.AllProjectReferences.Count;
		}

		/// <summary>
		/// Get DAG with all referenced projects of <paramref name="project"/> and the <paramref name="project"/> itself.
		/// Collection starts with the <paramref name="project"/>.
		/// </summary>
		/// <param name="project">The project to act on.</param>
		/// <returns>
		/// DAG with all referenced projects of <paramref name="project"/> and the <paramref name="project"/> itself.
		/// </returns>
		public static IReadOnlyCollection<Project> GetAllReferencedProjectsAndThis(this Project project) =>
			GetAllReferencedProjectsImpl(project, includeProject: true);

		/// <summary>
		/// Get DAG with all referenced projects of <paramref name="project"/>.
		/// </summary>
		/// <param name="project">The project to act on.</param>
		/// <returns>
		/// DAG with all referenced projects of <paramref name="project"/>.
		/// </returns>
		public static IReadOnlyCollection<Project> GetAllReferencedProjects(this Project project) =>
			GetAllReferencedProjectsImpl(project, includeProject: false);

		private static IReadOnlyCollection<Project> GetAllReferencedProjectsImpl(Project project, bool includeProject)
		{
			if (project.CheckIfNull().AllProjectReferences.Count == 0)
			{
				return includeProject
					? [project]
					: [];
			}

			var solution = project.Solution;
			var allReferencedProjects = includeProject
				? new List<Project>(2*project.AllProjectReferences.Count + 1) { project }
				: new List<Project>(2*project.AllProjectReferences.Count);

			var visitedProjects = new HashSet<ProjectId> { project.Id };
			var projectsToVisit = new Queue<ProjectId>(project.AllProjectReferences.Select(pr => pr.ProjectId));

			while (projectsToVisit.Count > 0)
			{
				var currentProjectId = projectsToVisit.Dequeue();

				if (!visitedProjects.Add(currentProjectId))
					continue;

				var currentProject = solution.GetProject(currentProjectId);

				if (currentProject == null)
					continue;

				allReferencedProjects.Add(currentProject);

				if (currentProject.AllProjectReferences.Count == 0)
					continue;

				foreach (var projectReference in currentProject.AllProjectReferences)
				{
					if (!visitedProjects.Contains(projectReference.ProjectId))
						projectsToVisit.Enqueue(projectReference.ProjectId);
				}
			}

			return allReferencedProjects;
		}

		public static IEnumerable<Project> GetDirectlyReferencedProjects(this Project project)
		{
			if (project.CheckIfNull().AllProjectReferences.Count == 0)
				return [];

			return project.AllProjectReferences
						  .Select(projectReference => project.Solution.GetProject(projectReference.ProjectId))
						  .Where(project => project != null)!;
		}
	}
}
