using System;
using System.Collections.Immutable;

using Acuminator.Runner.Input;

namespace Acuminator.Runner.Output
{
	/// <summary>
	/// Interface for the report outputters factory.
	/// </summary>
	internal interface IOutputterFactory
	{
		/// <summary>
		/// Create a report outputter.
		/// </summary>
		/// <param name="analysisContext">Context for the analysis.</param>
		/// <returns>
		/// The new report outputter.
		/// </returns>
		IReportOutputter CreateOutputter(AnalysisContext analysisContext);
	}
}
