using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

using Serilog;

namespace Acuminator.Runner.Output.PlainText
{
	/// <summary>
	/// The base class for the report outputter in the plain text format.
	/// </summary>
	internal abstract class PlainTextReportOutputterBase : IReportOutputter
	{
		public abstract void Dispose();

		public virtual void OutputReport(CodeSourceReport codeSourceReport, AnalysisContext analysisContext, CancellationToken cancellation)
		{
			codeSourceReport.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			WriteCodeSourceTitle($"{codeSourceReport.CodeSourceName}({Messages.TotalErrorsCountReportTitlePart}: {codeSourceReport.TotalDiagnosticsCount})");

			if (codeSourceReport.TotalDiagnosticsCount == 0)
				return;

			foreach (ProjectReport projectReport in codeSourceReport.ProjectReports)
			{
				OutputReport(projectReport, analysisContext, cancellation);

				if (projectReport.IsEmptyReport())
					WriteLine();
			}
		}

		public virtual void OutputReport(ProjectReport projectReport, AnalysisContext analysisContext, CancellationToken cancellation)
		{
			projectReport.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			WriteProjectTitle($"{projectReport.ProjectName}({Messages.ErrorsCountReportTitlePart}: {projectReport.TotalDiagnosticsCount}, " + 
							  $"{Messages.DistinctDiagnosticsReportTitlePart}: {projectReport.DistinctDiagnosticsCount})");

			if (projectReport.TotalDiagnosticsCount == 0)
				return;
			
			if (projectReport.ReportDetails != null)
			{
				OutputReportGroup(projectReport.ReportDetails, groupIndentationLevel: 0, recursionDepth: 0, cancellation);
			}
		}

		protected virtual void OutputReportGroup(ReportGroup reportGroup, int groupIndentationLevel, int recursionDepth, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			const int MaxRecursionDepth = 100;

			if (recursionDepth > MaxRecursionDepth)
			{
				Log.Error("Max recursion depth reached. The program execution most likely resulted in the stack overflow");
				return;
			}

			bool hasTitle = reportGroup.GroupTitle.HasValue;

			if (hasTitle)
			{
				WriteTitle(reportGroup.GroupTitle!.Value, groupIndentationLevel, reportGroup.TotalDiagnosticsCount,
						   reportGroup.DistinctDiagnosticsCount, reportGroup.HasContent);
			}

			cancellation.ThrowIfCancellationRequested();
			bool hasLines = OutputReportGroupLines(reportGroup, groupIndentationLevel, cancellation);

			cancellation.ThrowIfCancellationRequested();
			bool hasSubGroups = OutputReportGroupSubgroups(reportGroup, groupIndentationLevel, recursionDepth, cancellation);

			if (!hasSubGroups && (hasLines || hasTitle))
				WriteLine();
		}

		/// <summary>
		/// Output report group lines.
		/// </summary>
		/// <param name="reportGroup">The report group to use to output lines.</param>
		/// <param name="groupIndentationLevel">The indentation level of <paramref name="reportGroup"/>.</param>
		/// <param name="cancellation">Cancellation.</param>
		/// <returns>
		/// True if <paramref name="reportGroup"/> has lines, false if not.
		/// </returns>
		private bool OutputReportGroupLines(ReportGroup reportGroup, int groupIndentationLevel, CancellationToken cancellation)
		{
			if (reportGroup.Lines?.Count is null or 0)
				return false;

			int linesIndentationLevel;

			if (reportGroup.LinesTitle.HasValue)
			{
				WriteTitle(reportGroup.LinesTitle.Value, indentationLevel: groupIndentationLevel + 1, reportGroup.Lines.Count, 
						   reportGroup.DistinctDiagnosticsCount, reportGroup.HasContent);
				linesIndentationLevel = groupIndentationLevel + 2;
			}
			else
				linesIndentationLevel = groupIndentationLevel + 1;

			foreach (Line line in reportGroup.Lines)
			{
				cancellation.ThrowIfCancellationRequested();
				WriteLine(line, linesIndentationLevel);
			}

			return true;
		}

		private bool OutputReportGroupSubgroups(ReportGroup reportGroup, int groupIndentationLevel, int recursionDepth, CancellationToken cancellation)
		{
			if (reportGroup.ChildrenGroups?.Count is null or 0)
				return false;

			int subgroupIndentationLevel;

			if (reportGroup.ChildrenTitle.HasValue)
			{
				int totalSubGroupDiagnostics = reportGroup.ChildrenGroups.Sum(group => group.TotalDiagnosticsCount);

				WriteTitle(reportGroup.ChildrenTitle.Value, indentationLevel: groupIndentationLevel + 1, totalSubGroupDiagnostics,
						   reportGroup.DistinctDiagnosticsCount, reportGroup.HasContent);
				
				subgroupIndentationLevel = groupIndentationLevel + 2;
			}
			else
				subgroupIndentationLevel = groupIndentationLevel + 1;

			foreach (ReportGroup childGroup in reportGroup.ChildrenGroups)
			{
				cancellation.ThrowIfCancellationRequested();
				OutputReportGroup(childGroup, subgroupIndentationLevel, recursionDepth + 1, cancellation);
			}

			return true;
		}

		protected abstract void WriteCodeSourceTitle(string codeSourceTitle);

		protected abstract void WriteProjectTitle(string projectTitle);

		protected abstract void WriteTitle(in Title? title, int indentationLevel, int diagnosticsCount, int? distinctDiagnosticsCount, bool hasContent);

		protected void WriteLine<T>(T obj)
		{
			if (obj is null)
				WriteLine();
			else
				WriteLine(obj.ToString());
		}

		protected abstract void WriteLine();

		protected abstract void WriteLine(string text);

		protected abstract void WriteLine(in Line line, int indentationLevel);

		protected string GetPadding(int indentationLevel)
		{
			const int paddingMultiplier = 4;
			string padding = indentationLevel <= 0
				? string.Empty
				: new string(' ', indentationLevel * paddingMultiplier);

			return string.Intern(padding);
		}
	}
}