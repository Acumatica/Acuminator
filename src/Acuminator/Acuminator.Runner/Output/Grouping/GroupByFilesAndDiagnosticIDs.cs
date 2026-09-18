using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Output.Grouping
{
	/// <summary>
	/// Results grouper by any combination of file and diagnostic ID grouping modes.
	/// </summary>
	internal sealed class GroupByFilesAndDiagnosticIDs : GroupByDiagnosticsOrNoGrouping
	{
		public GroupByFilesAndDiagnosticIDs(GroupingMode grouping) : base(grouping)
		{ }

		/// <summary>
		/// Group reported diagnostics by source file containing the diagnostic.
		/// </summary>
		/// <param name="analysisContext">Analysis context.</param>
		/// <param name="diagnostics">Diagnostics to group.</param>
		/// <param name="projectDirectory">Pathname of the project directory.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// Acuminator diagnostics grouped by source file.
		/// </returns>
		public override IEnumerable<ReportGroup> GetGroupedDiagnostics(AnalysisContext analysisContext, IEnumerable<Diagnostic> diagnostics,
																	   string? projectDirectory, CancellationToken cancellation)
		{
			var diagnosticsGroupedByFiles = diagnostics.GroupBy(d => d.Location.SourceTree?.FilePath.NullIfWhiteSpace() ?? string.Empty)
													   .OrderBy(d => d.Key);

			foreach (var diagnosticsByFileGroup in diagnosticsGroupedByFiles)
			{
				cancellation.ThrowIfCancellationRequested();

				var diagnosticsByFile = diagnosticsByFileGroup.ToList();
				string fileName 	  = diagnosticsByFileGroup.Key.NullIfWhiteSpace() ?? Messages.NoFilePlaceholder;
				var fileGroup		  = GetGroupForFileDiagnostics(analysisContext, diagnosticsByFile, projectDirectory, fileName, cancellation);

				if (fileGroup != null)
					yield return fileGroup;
			}
		}

		private ReportGroup? GetGroupForFileDiagnostics(AnalysisContext analysisContext, List<Diagnostic> diagnostics, 
														string? projectDirectory, string fileName, CancellationToken cancellation)
		{
			bool groupByDiagnosticID = analysisContext.GroupingMode.HasGrouping(GroupingMode.DiagnosticIDs);

			if (groupByDiagnosticID)
			{
				var subGroups = GetSubGroupsByDiagnosticID(analysisContext, diagnostics, projectDirectory, cancellation);
				var fileGroup = new ReportGroup
				{
					GroupTitle 				 = new Title(fileName, TitleKind.File),
					TotalDiagnosticsCount 	 = diagnostics.Count,
					DistinctDiagnosticsCount = subGroups.Count,
					ChildrenGroups 			 = subGroups.NullIfEmpty()
				};

				return fileGroup;
			}
			else
			{
				return CreateGroupByFileOnly(fileName, analysisContext, diagnostics, projectDirectory);
			}
		}

		private IReadOnlyCollection<ReportGroup> GetSubGroupsByDiagnosticID(AnalysisContext analysisContext, List<Diagnostic> diagnostics,
																			string? projectDirectory, CancellationToken cancellation)
		{
			var subGroups = GroupDiagnosticsByDiagnosticIdOrNothing(analysisContext, diagnostics, projectDirectory, 
																	sortBySourceFile: false, sortBySeverity: false, sortByDiagnosticId: true, cancellation);
			return subGroups;
		}

		private ReportGroup CreateGroupByFileOnly(string fileName, AnalysisContext analysisContext, List<Diagnostic> diagnostics, string? projectDirectory)
		{
			int distinctDiagnosticsCount = diagnostics.GroupBy(d => d.Id)
													  .Count();
			var lines = GetOrderedDiagnosticsWithLocationLines(diagnostics, projectDirectory, analysisContext,
															   sortBySourceFile: false, sortBySeverity: false, sortByDiagnosticId: true)
													   .ToList(diagnostics.Count);
			var fileGroup = new ReportGroup
			{
				GroupTitle 				 = new Title(fileName, TitleKind.File),
				TotalDiagnosticsCount	 = diagnostics.Count,
				DistinctDiagnosticsCount = distinctDiagnosticsCount,
				Lines 					 = lines.NullIfEmpty()
			};

			return fileGroup;
		}
	}
}
