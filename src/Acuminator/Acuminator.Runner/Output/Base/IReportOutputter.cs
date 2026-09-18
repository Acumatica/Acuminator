using System;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;

namespace Acuminator.Runner.Output
{
	/// <summary>
	/// Interface for the report outputter.
	/// </summary>
	internal interface IReportOutputter : IDisposable
	{
		/// <summary>
		/// Outputs the code source report.
		/// </summary>
		/// <param name="codeSourceReport">The code source report.</param>
		/// <param name="analysisContext">The analysis context.</param>
		/// <param name="cancellation">Cancellation token.</param>
		void OutputReport(CodeSourceReport codeSourceReport, AnalysisContext analysisContext, CancellationToken cancellation);
	}
}
