using System;
using System.Linq;

namespace Acuminator.Utilities;

/// <summary>
/// The type of the host in which the analysis is performed. This is used to determine the way how settings are retrieved, whether from shared memory or from global settings.
/// </summary>
public enum AnalysisHostType
{
	/// <summary>
	/// The host is not specified explicitly. This is a possible option for VS code analysis worker process, unit tests, and Nuget analyzers.
	/// </summary>
	NotSpecified = 0,

	/// <summary>
	/// The host is Visual Studio.
	/// </summary>
	VisualStudio,

	/// <summary>
	/// The host is the Acuminator console runner.
	/// </summary>
	Runner
}
