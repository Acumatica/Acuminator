using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Runner.Constants
{
	internal static class CommandLineArgNames
	{
		public const string CodeSource = "codeSource";

		public const char VerbosityShort = 'v';
		public const string VerbosityLong = "verbosity";

		public const string DisableSuppressionMechanism = "disable-suppression";
		public const char ReportGroupingShort = 'g';
		public const string ReportGroupingLong = "grouping";

		public const string OutputAbsolutePathsToErrors = "output-absolute-paths-for-errors";

		public const char OutputFileShort = 'f';
		public const string OutputFileLong = "file";

		public const string OutputFormat = "format";

		public const string IsvSpecificAnalysisIsEnabled = "isv-mode";

		public const string PX1007DiagnosticIsEnabled = "enable-PX1007";

		public const string DisablePX1099Diagnostic = "disable-PX1099";

		public const string BannedApiFilePath = "banned-APIs-path";

		public const string AllowedApisFilePath = "allowed-APIs-path";

		public const char AcuminatorWorkModeShort  = 'w';
		public const string AcuminatorWorkModeLong = "work-mode";

		public const string EnableInformationalDiagnostics = "enable-info-diagnostics";
		public const string UseNonInteractiveMode = "non-interactive";

		public static class WorkModes
		{
			public const string ReportErrors = "report-errors";
			public const string GenerateSuppressionFile = "generate-suppressions";
			public const string ReportErrorsAndGenerateSuppresionFile = "report-and-generate";
		}
	}
}