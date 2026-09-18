using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;

using Acuminator.Runner.Input;
using Acuminator.Runner.Output.Data;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Output.Json
{
	/// <summary>
	/// JSON report outputter base class.
	/// </summary>
	internal abstract class JsonReportOutputterBase : IReportOutputter
	{
		public abstract void Dispose();

		public virtual void OutputReport(CodeSourceReport codeSourceReport, AnalysisContext analysisContext, CancellationToken cancellation)
		{
			codeSourceReport.ThrowOnNull();
			analysisContext.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			var options = GetJsonSerializerOptions();
			string serializedReport = JsonSerializer.Serialize(codeSourceReport, options);

			cancellation.ThrowIfCancellationRequested();
			OutputReportText(serializedReport);
		}

		public virtual void OutputReport(ProjectReport projectReport, AnalysisContext analysisContext, CancellationToken cancellation)
		{
			projectReport.ThrowOnNull();
			analysisContext.ThrowOnNull();
			cancellation.ThrowIfCancellationRequested();

			var options = GetJsonSerializerOptions();
			string serializedReport = JsonSerializer.Serialize(projectReport, options);

			cancellation.ThrowIfCancellationRequested();
			OutputReportText(serializedReport);
		}

		protected virtual JsonSerializerOptions GetJsonSerializerOptions() =>
			new()
			{
				WriteIndented = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

		protected abstract void OutputReportText(string serializedReport);
	}
}