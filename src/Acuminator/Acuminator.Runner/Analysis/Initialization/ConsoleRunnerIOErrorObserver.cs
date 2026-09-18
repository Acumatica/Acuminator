using System;
using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.IO;

using Serilog;

namespace Acuminator.Runner.Analysis.Initialization
{
	internal class ConsoleRunnerIOErrorObserver(ILogger? logger) : IIOErrorProcessor
	{
		private readonly ILogger? _logger = logger;

		public void ProcessError(Exception exception, [CallerMemberName] string? reportedFrom = null)
		{
			if (reportedFrom.IsNullOrWhiteSpace())
			{
				_logger?.Error(exception, "The Acuminator thrown an IO exception");
			}
			else
			{
				_logger?.Error(exception, "The Acuminator thrown an IO exception from {ReportedFrom} method", reportedFrom);
			}
		}
	}
}
