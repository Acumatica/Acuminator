using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Output.Grouping;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Output
{
    /// <summary>
    /// The project report builder's default implementation.
    /// </summary>
    internal class ProjectReportBuilder : IProjectReportBuilder
	{
		public ProjectReport BuildReport(IEnumerable<Diagnostic> foundDiagnostics, AnalysisContext analysisContext, 
										 Project project, CancellationToken cancellation)
		{
			var diagnosticsArray = foundDiagnostics?.ToImmutableArray() ?? [];
			return BuildReport(diagnosticsArray, analysisContext, project, cancellation);
		}

		public ProjectReport BuildReport(ImmutableArray<Diagnostic> foundDiagnostics, AnalysisContext analysisContext, 
										 Project project, CancellationToken cancellation)
		{
			project.ThrowOnNull();
			analysisContext.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			string? projectDirectory = GetProjectDirectory(project);
			int distinctErrorsCount  = !foundDiagnostics.IsDefaultOrEmpty
				? foundDiagnostics.GroupBy(d => d.Id).Count()
				: 0;

			var mainReportGroup = GetMainReportGroupFromAllDiagnostics(foundDiagnostics, analysisContext, projectDirectory, 
																	   distinctErrorsCount, cancellation);
			var report = new ProjectReport(project.Name)
			{
				TotalDiagnosticsCount	 = foundDiagnostics.Length,
				DistinctDiagnosticsCount = distinctErrorsCount,
				ReportDetails			 = mainReportGroup,
			};

			return report;
		}

		private string? GetProjectDirectory(Project project)
		{
			if (project.FilePath.IsNullOrWhiteSpace())
				return null;

			string projectFile = Path.GetFullPath(project.FilePath.Trim());
			string projectDirectory = Path.GetDirectoryName(projectFile);
			return projectDirectory;
		}

		protected virtual ReportGroup GetMainReportGroupFromAllDiagnostics(ImmutableArray<Diagnostic> foundDiagnostics, AnalysisContext analysisContext,
																		   string? projectDirectory, int distinctErrorsCount, CancellationToken cancellation)
		{
			var diagnosticGroups = GetAllReportGroups(foundDiagnostics, analysisContext, projectDirectory, cancellation).ToList();

			var mainApiGroup = new ReportGroup()
			{
				TotalDiagnosticsCount    = foundDiagnostics.Length,
				DistinctDiagnosticsCount = distinctErrorsCount,

				ChildrenTitle  = new Title(Messages.FoundErrorsReportSubtitle, TitleKind.AllDiagnostics),
				ChildrenGroups = diagnosticGroups.NullIfEmpty(),
			};

			return mainApiGroup;
		}

		protected virtual IEnumerable<ReportGroup> GetAllReportGroups(ImmutableArray<Diagnostic> foundDiagnostics, AnalysisContext analysisContext,
																	  string? projectDirectory, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var linesGrouper	   = GetLinesGrouper(analysisContext.GroupingMode);
			var groupedDiagnostics = linesGrouper.GetGroupedDiagnostics(analysisContext, foundDiagnostics, projectDirectory, cancellation);
			return groupedDiagnostics;
		}

		protected virtual IGroupLines GetLinesGrouper(GroupingMode groupingMode) =>
			groupingMode.HasGrouping(GroupingMode.Files)
				? new GroupByFilesAndDiagnosticIDs(groupingMode)
				: new GroupByDiagnosticsOrNoGrouping(groupingMode);
	}
}