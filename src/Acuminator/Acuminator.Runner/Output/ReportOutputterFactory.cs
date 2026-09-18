using System;
using System.Collections.Generic;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Output.Json;
using Acuminator.Runner.Output.PlainText;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Output
{
    /// <summary>
    /// The standard output formatter.
    /// </summary>
    internal class ReportOutputterFactory : IOutputterFactory
	{
		public IReportOutputter CreateOutputter(AnalysisContext analysisContext)
		{
			analysisContext.ThrowOnNull();

			if (analysisContext.OutputFormat == OutputFormat.PlainText)
			{
				return analysisContext.OutputFileName.IsNullOrWhiteSpace()
					? new PlainTextReportOutputterConsole()
					: new PlainTextReportOutputterFile(analysisContext.OutputFileName);
			}
			else if (analysisContext.OutputFormat == OutputFormat.Json)
			{
				return analysisContext.OutputFileName.IsNullOrWhiteSpace()
					? new JsonReportOutputterToConsole()
					: new JsonReportOutputterToFile(analysisContext.OutputFileName);
			}
			else
				throw new NotSupportedException();
		}
	}
}