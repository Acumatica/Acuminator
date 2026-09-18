using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Utilities;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

using DiagnosticInfo = (Microsoft.CodeAnalysis.Diagnostic Diagnostic, string Message, string Location);

namespace Acuminator.Runner.Output.Grouping
{
	/// <summary>
	/// Base class to group Acuminator diagnostics.
	/// </summary>
	internal abstract class GroupLinesBase : IGroupLines
	{
		/// <summary>
		/// The diagnostics' grouping mode.
		/// </summary>
		public GroupingMode Grouping { get; }

		protected GroupLinesBase(GroupingMode grouping)
		{
			Grouping = grouping;
		}

		/// <inheritdoc cref="IGroupLines.GetGroupedDiagnostics(AnalysisContext, IEnumerable{Diagnostic}, string?, CancellationToken)"/>
		public abstract IEnumerable<ReportGroup> GetGroupedDiagnostics(AnalysisContext analysisContext, IEnumerable<Diagnostic> diagnostics,
																	   string? projectDirectory, CancellationToken cancellation);

		/// <summary>
		/// Gets the ordered diagnostics with location report lines for given <paramref name="unsortedDiagnostics"/>.<br/>
		/// If <paramref name="sortBySourceFile"/> is true, the diagnostics are sorted additionally by source file. This sorting is always applied first.<br/>
		/// If <paramref name="sortByDiagnosticId"/> is true, the diagnostics are sorted additionally by diagnostic identifier. This sorting is applied after sorting by source file.<br/>
		/// </summary>
		/// <param name="unsortedDiagnostics">The unsorted diagnostics.</param>
		/// <param name="projectDirectory">Pathname of the project directory.</param>
		/// <param name="analysisContext">Context for the analysis.</param>
		/// <param name="sortBySourceFile">True to sort by source file. This sorting is always applied first.</param>
		/// <param name="sortBySeverity">True to sort by diagnostic severity. This sorting is applied after sorting by source file.</param>
		/// <param name="sortByDiagnosticId">True to sort by diagnostic identifier. This sorting is applied after sortings by source file and diagnostic severity.</param>
		/// <returns>
		/// The ordered diagnostics with location report lines for given <paramref name="unsortedDiagnostics"/>.
		/// </returns>
		protected IEnumerable<Line> GetOrderedDiagnosticsWithLocationLines(IEnumerable<Diagnostic> unsortedDiagnostics, string? projectDirectory, 
																		   AnalysisContext analysisContext, 
																		   bool sortBySourceFile, bool sortBySeverity, bool sortByDiagnosticId)
		{
			var unsortedDiagnosticInfos = unsortedDiagnostics.Select(d => (Diagnostic: d,
																		   Message: GetDiagnosticMessage(d),
																		   Location: GetPrettyLocation(d, projectDirectory, analysisContext)));

			var sortedDiagnosticInfos = GetSortedDiagnosticInfos(sortBySourceFile, sortBySeverity, sortByDiagnosticId, unsortedDiagnosticInfos);
			var reportLines	= sortedDiagnosticInfos.Select(d  => new Line(GetSeverityLineSpan(d.Diagnostic.Severity), 
																		  new LineSpan(d.Diagnostic.Id),
																		  new LineSpan(d.Message), 
																		  new LineSpan(d.Location)));
			return reportLines;
		}

		private LineSpan GetSeverityLineSpan(DiagnosticSeverity severity)
		{
			var color = GetColorForSeverity(severity);
			return new LineSpan(severity.ToString(), color);
		}

		private ConsoleColor? GetColorForSeverity(DiagnosticSeverity severity) => severity switch
		{
			DiagnosticSeverity.Error   => ConsoleColor.Red,
			DiagnosticSeverity.Warning => ConsoleColor.DarkYellow,
			DiagnosticSeverity.Info    => ConsoleColor.Cyan,
			_ 						   => null
		};

		private IEnumerable<DiagnosticInfo> GetSortedDiagnosticInfos(bool sortBySourceFile, bool sortBySeverity, bool sortByDiagnosticId, 
																	 IEnumerable<DiagnosticInfo> unsortedDiagnosticInfos)
		{
			IOrderedEnumerable<DiagnosticInfo>? sortedDiagnosticInfos = null;

			if (sortBySourceFile)
				sortedDiagnosticInfos = unsortedDiagnosticInfos.OrderBy(d => d.Diagnostic.Location.SourceTree?.FilePath ?? string.Empty);

			if (sortBySeverity)
			{
				sortedDiagnosticInfos = sortedDiagnosticInfos?.ThenByDescending(d => d.Diagnostic.Severity) ?? 
										unsortedDiagnosticInfos.OrderByDescending(d => d.Diagnostic.Severity);
			}
			
			if (sortByDiagnosticId)
			{
				sortedDiagnosticInfos = sortedDiagnosticInfos?.ThenBy(d => d.Diagnostic.Id) ?? 
										unsortedDiagnosticInfos.OrderBy(d => d.Diagnostic.Id);
			}

			sortedDiagnosticInfos = sortedDiagnosticInfos?.ThenBy(d => d.Location) ??
									unsortedDiagnosticInfos.OrderBy(d => d.Location);
			sortedDiagnosticInfos = sortedDiagnosticInfos.ThenBy(d => d.Message, StringComparer.Ordinal);
			return sortedDiagnosticInfos;
		}

		protected string GetPrettyLocation(Diagnostic diagnostic, string? projectDirectory, AnalysisContext analysisContext)
		{
			string prettyLocation = diagnostic.Location.GetMappedLineSpan().ToString();

			if (analysisContext.OutputAbsolutePathsToUsages || projectDirectory.IsNullOrWhiteSpace())
				return prettyLocation.Enquote();

			StringComparison stringComparison = analysisContext.CaseSensitiveFilePaths
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

			if (!prettyLocation.StartsWith(projectDirectory, stringComparison))
				return prettyLocation.Enquote();

			string relativeLocation = "." + prettyLocation.Substring(projectDirectory.Length);
			return relativeLocation.Enquote();
		}

		protected string GetDiagnosticMessage(Diagnostic diagnostic)
		{
			string message = diagnostic.GetMessage(CultureInfo.CurrentCulture).NullIfWhiteSpace() ??
							 diagnostic.GetMessage(CultureInfo.InvariantCulture).NullIfWhiteSpace() ??
							 diagnostic.Descriptor.Title.ToString(CultureInfo.InvariantCulture);
			return message;
		}
	}
}
