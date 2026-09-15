using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;

using Microsoft.CodeAnalysis;

namespace Acuminator.Runner.Output
{
	/// <summary>
	/// Interface for the project report builder.
	/// </summary>
	internal interface IProjectReportBuilder
	{
		/// <summary>
		/// Builds the report from the diagnostics.
		/// </summary>
		/// <param name="foundDiagnostics">Diagnostics reported by Acuminator for the <paramref name="project"/>.</param>
		/// <param name="analysisContext">The analysis context.</param>
		/// <param name="project">The project.</param>
		/// <param name="cancellation">Cancellation token.</param>
		ProjectReport BuildReport(IEnumerable<Diagnostic> foundDiagnostics, AnalysisContext analysisContext, Project project,
								  CancellationToken cancellation);

		/// <summary>
		/// Builds the report from the diagnostics.
		/// </summary>
		/// <param name="foundDiagnostics">Diagnostics reported by Acuminator for the <paramref name="project"/>.</param>
		/// <param name="analysisContext">The analysis context.</param>
		/// <param name="project">The project.</param>
		/// <param name="cancellation">Cancellation token.</param>
		ProjectReport BuildReport(ImmutableArray<Diagnostic> foundDiagnostics, AnalysisContext analysisContext, Project project,
								  CancellationToken cancellation);
	}
}
