using System;
using System.Collections.Generic;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Output.Grouping
{
	/// <summary>
	/// Interface for helpers that group found diagnostics.
	/// </summary>
	internal interface IGroupLines
	{
		/// <summary>
		/// The grouping mode of the grouper.
		/// </summary>
		GroupingMode Grouping { get; }

		/// <summary>
		/// Get Acuminator diagnostics grouped according to the <see cref="Grouping"/> mode.
		/// </summary>
		/// <param name="analysisContext">Analysis context.</param>
		/// <param name="diagnostics">Diagnostics to group.</param>
		/// <param name="projectDirectory">Pathname of the project directory.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// Acuminator diagnostics grouped according to the <see cref="Grouping"/> mode.
		/// </returns>
		IEnumerable<ReportGroup> GetGroupedDiagnostics(AnalysisContext analysisContext, IEnumerable<Diagnostic> diagnostics,
													   string? projectDirectory, CancellationToken cancellation);
	}
}
