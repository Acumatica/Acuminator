using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Runner.Constants;

using CommandLine;

using Microsoft.CodeAnalysis;

using Serilog.Events;

namespace Acuminator.Runner.Input
{
	internal class CommandLineOptions
	{
		/// <summary>
		/// The code source to be analyzed by Acuminator.
		/// </summary>
		/// <remarks>
		/// Currently, the supported code sources are C# projects and C# solutions.
		/// </remarks>
		[Value(index: 0, MetaName = CommandLineArgNames.CodeSource, Required = true,
			   HelpText = """
			A path to the "code source" to be validated. The term "code source" is a generalization for components or services that can provide source code to the Acuminator Console Runner.
			Currently, the supported code sources are C# projects and C# solutions.
			""")]
		public string CodeSource { get; }

		/// <summary>
		/// Optional explicitly specified logger <see cref="LogEventLevel"/> verbosity for tool's own messages. <br/>
		/// If null then <see cref="LogEventLevel.Information"/> will be used as default.
		/// </summary>
		/// <value>
		/// The explicitly specified logger's verbosity.
		/// </value>
		[Option(shortName: CommandLineArgNames.VerbosityShort, longName: CommandLineArgNames.VerbosityLong,
				HelpText = "Optional. Specifies logger verbosity. The allowed values are taken from the " + 
						  $"\"{nameof(Serilog)}.{nameof(Serilog.Events)}.{nameof(LogEventLevel)}\" enum.\r\n" +
						  $"""

						  The available values:
						   - "{nameof(LogEventLevel.Verbose)}", 
						   - "{nameof(LogEventLevel.Debug)}", 
						   - "{nameof(LogEventLevel.Information)}",
						   - "{nameof(LogEventLevel.Warning)}",
						   - "{nameof(LogEventLevel.Error)}",
						   - "{nameof(LogEventLevel.Fatal)}".

						  By default, the logger uses the "{nameof(LogEventLevel.Information)}" verbosity."
						  """)]
		public string? Verbosity { get; }

		/// <summary>
		/// When this optional flag is specified, the code analysis will report Acuminator errors suppressed with Acuminator suppression mechanisms.
		/// </summary>
		[Option(longName: CommandLineArgNames.DisableSuppressionMechanism, Default = false,
				HelpText = "Optional. A flag that indicates that the code analysis will report Acuminator errors suppressed with Acuminator suppression mechanisms.")]
		public bool DisableSuppressionMechanism { get; }

		/// <summary>
		/// This optional parameter allows you to provide explicitly a path to the MSBuild tool that will be used for analysis.
		///	By default, MSBuild installations will be searched automatically on the current machine and the latest found version will be used.
		/// </summary>
		/// <value>
		/// The optional explicitly specified path to MSBuild.
		/// </value>
		[Option(longName: CommandLineArgNames.MSBuildPath,
				HelpText = """
						   Optional. Provides an explicit path to the MSBuild tool that will be used for analysis. 
						   By default, MSBuild installations are detected automatically on the current machine and the latest found version will be used.
						   """)]
		public string? MSBuildPath { get; }

		/// <summary>
		/// The path to the output file. If not specified then the report with analysis results will be outputted to the console window.
		/// </summary>
		[Option(shortName: CommandLineArgNames.OutputFileShort, longName: CommandLineArgNames.OutputFileLong,
				HelpText = "Optional. A path to the output file. If not specified, the report with analysis results will be outputted to the console window.")]
		public string? OutputFileName { get; }

		/// <summary>
		/// This flag regulates how the locations of found diagnostics will be outputted.<br/>
		///	By default, file paths in locations are relative to the containing project directory.<br/>
		///	However, if this flag is set, then the absolute file paths will be used.
		/// </summary>
		[Option(longName: CommandLineArgNames.OutputAbsolutePathsToErrors, Default = false,
				HelpText = """
						   Optional. A flag that regulates how the locations of errors are outputted. 
						   By default, the report contains file paths relative to the containing project directory. 
						   However, if this flag is set, the absolute file paths is used.
						   """)]
		public bool OutputAbsolutePathsToUsages { get; }

		/// <summary>
		/// The report output format. There are two supported values:
		/// <list type="bullet">
		/// <item>"text" to output the report in plain text, this is the default output mode,</item>
		/// <item>"json" to output the report in JSON format.</item>
		/// </list>
		/// </summary>
		[Option(longName: CommandLineArgNames.OutputFormat,
				HelpText = """
						   Optional. The output format of the report. The following values are supported:
						   - "text" to output the report in plain text. This is the default output mode.
						   - "json" to output the report in the JSON format.
						   """)]
		public string? OutputFormat { get; }

		/// <summary>
		/// This flag indicates whether the ISV analysis mode is enabled.<br/>
		/// By default, the ISV mode is disabled. Developers should explicitly enable the ISV mode by setting this flag.
		/// </summary>
		/// <remarks>
		/// Acuminator has two analysis modes:
		/// <list type="bullet">
		/// <item>The default mode (also called "non-ISV mode") used by Acumatica developers internally</item>
		/// <item>
		/// The ISV-mode recommended for ISV and other external developers that create customizations based on Acumatica Framework.<br/><br/>
		/// The ISV mode got its name from the ISV certification process that is used by Acumatica to validate third party customizations created by Integrated Software Vendors (ISVs).<br/>
		/// A part of the ISV certification process is the execution of Acuminator analysis in this mode for the code of the customization being validated.
		/// </item>
		/// </list> 
		/// The difference between these two modes is that the analysis in the ISV mode is stricter than in the default mode.<br/>
		/// In the ISV mode Acuminator performs additional diagnostics that are not used in the default mode. The severity of some diagnostics is increased from "Warning" to "Error".<br/>
		/// For example, in the ISV mode Acuminator will report usages of Acumatica Framework APIs that are marked for internal use only. 
		/// </remarks>
		[Option(longName: CommandLineArgNames.IsvSpecificAnalysisIsEnabled, Default = false,
				HelpText = """
							Optional. A flag that enables ISV-specific analysis mode.
							By default, the ISV mode is disabled. Developers should explicitly enable the ISV mode by setting this flag.

							Acuminator has two analysis modes:
							- The default mode (also called "non-ISV mode") used by Acumatica developers internally
							- The ISV-mode recommended for ISV and other external developers that create customizations based on Acumatica Framework

							  The ISV mode got its name from the ISV certification process that is used by Acumatica to validate third-party customizations created by Integrated Software Vendors (ISVs).
							  A part of the ISV certification process is execution of Acuminator analysis in this mode for the customization code that is being validated.

							The difference between these two modes is that the analysis in the ISV mode is stricter than in the default mode.
							In the ISV mode, Acuminator performs additional diagnostics that are not used in the default mode. The severity of some diagnostics is increased from "Warning" to "Error".
							For example, in the ISV mode, Acuminator reports the use of Acumatica Framework APIs that are marked for internal use only.
							""")]
		public bool IsvSpecificAnalysisIsEnabled { get; }

		/// <summary>
		/// This flag indicates whether the PX1007 diagnostic is enabled.<br/>
		/// By default, the PX1007 diagnostic is disabled. Developers should explicitly enable it by specifying this flag.
		/// </summary>
		/// <remarks>
		/// The PX1007 diagnostic is used to check whether the XML documentation comment is present on DACs and DAC field properties.<br/>
		/// You can find more details about this diagnostic here: https://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1007.md.
		/// <br/>
		/// This diagnostic is important for Acumatica internal development process, but it is considered as an optional diagnostic for ISV, and other external developers.<br/>
		/// Therefore, by default it is disabled.
		/// </remarks>
		[Option(longName: CommandLineArgNames.PX1007DiagnosticIsEnabled, Default = false,
				HelpText = """
						   Optional. A flag that enables the PX1007 diagnostic. 
						   This diagnostic checks the presence of XML documentation comments on DACs and DAC fields. 

						   By default, the PX1007 diagnostic is disabled. Developers should explicitly enable it by specifying this flag.

						   This diagnostic is important for Acumatica internal development process, but it is considered as an optional diagnostic for ISV, and other external developers.
						   Therefore, by default it is disabled.

						   You can find more details about this diagnostic here: https://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1007.md.
						   """)]
		public bool PX1007DiagnosticIsEnabled { get; }

		/// <summary>
		/// This flag indicates whether the PX1099 diagnostic for banned API should be disabled.<br/>
		/// By default, the PX1099 diagnostic is enabled. Developers should explicitly disable it by specifying this flag.
		/// </summary>
		/// <remarks>
		/// The PX1099 diagnostic detects APIs that should not be used with the Acumatica Framework. Each banned API may have its own reason for being banned.<br/>
		/// <br/>
		/// The PX1099 diagnostic checks every API call in the code against a list of forbidden APIs and a list of allowed APIs.<br/>
		/// These lists can be loaded from custom files specified by the <see cref="BannedApiFilePath"/> and <see cref="AllowedApisFilePath"/> options respectively.<br/>
		/// If no file is specified, a default list of APIs will be used. You can find the content of default API lists and how exactly the diagnostic works in the documentation:
		/// https://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1099.md.
		/// </remarks>
		[Option(longName: CommandLineArgNames.DisablePX1099Diagnostic, Default = false,
				HelpText = """
						   Optional. A flag that disables the PX1099 diagnostic for banned APIs.
						   By default, the PX1099 diagnostic is enabled. Developers should explicitly disable it by specifying this flag.

						   The PX1099 diagnostic detects APIs that should not be used with the Acumatica Framework. Each banned API may have its own reason for being banned.

						   The PX1099 diagnostic checks every API call in the code against a list of forbidden APIs and a list of allowed APIs.
						   These lists can be loaded from custom files specified in the "BannedApiFilePath" and "AllowedApisFilePath" options respectively.
						   If no file is specified, Acuminator will use the default list of APIs. You can find the contents of default API list and how exactly the diagnostic works in the documentation:
						   https://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1099.md.
						   """)]
		public bool DisablePX1099Diagnostic { get; }

		/// <summary>
		/// A path to a custom file with a list of forbidden APIs for the PX1099 diagnostic.<br/>
		/// This is an optional parameter that allows developers to override a default list of forbidden APIs for PX1099 diagnostic.<br/>
		/// </summary>
		/// <remarks>
		/// The PX1099 diagnostic checks every API call in the code against a list of forbidden APIs and a list of allowed APIs.<br/>
		/// The diagnostic supports custom externally provided list of forbidden APIs which can be loaded from a file.<br/>
		/// If a file with forbidden APIs is not specified, a default list of APIs embedded into Acuminator will be used.<br/>
		/// You can find the default list of forbidden APIs in the documentation:<br/>
		/// https://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1099.md#banned-and-allowed-apis.
		/// </remarks>
		[Option(longName: CommandLineArgNames.BannedApiFilePath,
				HelpText = """
						   Optional. A path to a file with a custom list of banned APIs for the PX1099 diagnostic.
						   By using this parameters, developers can override the default list of forbidden APIs for the PX1099 diagnostic.

						   The PX1099 diagnostic checks every API call in the code against a list of forbidden APIs and a list of allowed APIs.
						   The diagnostic supports custom externally provided lists of forbidden APIs which can be loaded from a file.
						   If a file with forbidden APIs is not specified, Acuminator will use the default list of APIs embedded into Acuminator.
						   You can find the default list of forbidden APIs in the documentation:
						   https://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1099.md#banned-and-allowed-apis.
						   """)]
		public string? BannedApiFilePath { get; }

		/// <summary>
		/// A path to a custom file with a list of allowed APIs for the PX1099 diagnostic.<br/>
		/// This is an optional parameter that allows developers to override a default list of allowed APIs for PX1099 diagnostic.<br/>
		/// </summary>
		/// <remarks>
		/// The PX1099 diagnostic checks every API call in the code against a list of forbidden APIs and a list of allowed APIs.<br/>
		/// The diagnostic supports custom externally provided list of allowed APIs which can be loaded from a file.<br/>
		/// If a file with allowed APIs is not specified, a default list of APIs embedded into Acuminator will be used.<br/>
		/// You can find the default list of allowed APIs in the documentation:<br/>
		/// https://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1099.md#banned-and-allowed-apis.
		/// </remarks>
		[Option(longName: CommandLineArgNames.AllowedApisFilePath,
				HelpText = """
						   Optional. A path to a file with a custom list of allowed APIs for the 1099 diagnostic.
						   by using this parameter, developers can override a default list of allowed APIs for the PX1099 diagnostic.

						   The PX1099 diagnostic checks every API call in the code against a list of forbidden APIs and a list of allowed APIs.
						   The diagnostic supports custom externally provided lists of allowed APIs which can be loaded from a file.
						   If a file with allowed APIs is not specified, Acuminator will use the default list of APIs embedded into Acuminator.
						   You can find the default list of allowed APIs in the documentation:
						   https://github.com/Acumatica/Acuminator/blob/dev/docs/diagnostics/PX1099.md#banned-and-allowed-apis.
						   """)]
		public string? AllowedApisFilePath { get; }

		/// <summary>
		/// This option specifies the mode in which Acuminator should work. There are three available work modes:<br/>
		/// <list type="bullet">
		/// <item>
		/// "<c>report-errors</c>"(<see cref="CommandLineArgNames.WorkModes.ReportErrors"/>).<br/>
		/// In this mode Acuminator report errors if they are not suppressed with Acuminator suppression mechanisms,<br/>
		/// or if the suppression mechanisms are disabled with <see cref="DisableSuppressionMechanism"/> flag.<br/>
		/// This is the default Acuminator work mode, if no work mode is specified explicitly.
		/// </item>
		/// <item>
		/// "<c>generate-suppressions</c>"(<see cref="CommandLineArgNames.WorkModes.GenerateSuppressionFile"/>).<br/>
		/// In this mode Acuminator does not report errors. Instead it generates suppression records in Acuminator suppression file for all errors it found in the code.
		/// </item>
		/// <item>
		/// "<c>report-and-generate</c>"(<see cref="CommandLineArgNames.WorkModes.ReportErrorsAndGenerateSuppresionFile"/>).<br/>
		/// In this mode Acuminator will both report errors and generate suppression records in Acuminator suppression file. It is a combination of two other modes.
		/// </item>
		/// </list>
		/// </summary>
		/// <remarks>
		/// Acuminator provides two mechanisms to suppress its diagnostics:<br/>
		/// - Local suppression with a suppression comment.<br/>
		/// - Global suppression with a suppression file.<br/>
		/// <br/>
		/// Each of the mechanisms is used for different scenarios. Local suppression provides a notice to the reader that there is an Acuminator alert suppressed due to the specified reasons.<br/>
		/// Global suppression does not provide any information regarding alerts suppressed in the code or the reason for the suppression of the alert to the reader.<br/>
		/// It is useful for development processes in large codebases where the number of errors is large and the cost of fixing them is high.<br/>
		/// For example, you can use the Acuminator Console Runner in CI scenarios to run automatic tests with Acuminator static analysis<br/>
		/// which will rely on Acuminator suppression file for the main code base and report Acuminator warnings and errors only for the new code.<br/>
		/// <br/>
		/// Acuminator work modes are designed to provide better support for automated CI scenarios.<br/>
		/// In most cases the regular analysis is used which simply outputs unsuppressed errors. Therefore, it is a default work mode.<br/>
		/// <br/>
		///	The suppression file generator mode is designed to support CI scenarios by implementing automatic generation of the suppression file for a given codebase.<br/>
		///	It does not output found errors. Instead it will generate suppression records for all errors it found in the code and add them to the suppression file.<br/>
		/// Note that it does not generate suppression records for errors that are already suppressed in the code with local suppression comments.<br/>
		/// <br/>
		/// The third work mode combines both output of errors and suppression file generation. It can be used in scenarios like Acumatica tests<br/>
		/// where the same integration test can be used both to find errors in the new code and generate new suppression file for a project in a convenient way from the UI.<br/>
		/// This work mode allows to both enhance the outputted report with precise locations of found errors and keep generating the suppression file all in scope of one test. 
		/// </remarks>
		[Option(shortName: CommandLineArgNames.AcuminatorWorkModeShort, longName: CommandLineArgNames.AcuminatorWorkModeLong, 
				Default = CommandLineArgNames.WorkModes.ReportErrors,
				HelpText = $"""
						   Optional. The mode in which Acuminator should work. The following work modes are available:
						   - "{CommandLineArgNames.WorkModes.ReportErrors}"
						   In this mode, Acuminator report errors if they are not suppressed with Acuminator suppression mechanisms,
						   or if the suppression mechanism is disabled with the "{CommandLineArgNames.DisableSuppressionMechanism}" flag.
						   This is the default Acuminator work mode, if no work mode is specified explicitly.
						   - "{CommandLineArgNames.WorkModes.GenerateSuppressionFile}"
						   In this mode, Acuminator does not report errors. Instead, it generates suppression records in the Acuminator suppression file for all errors that have been found in the code.
						   - "{CommandLineArgNames.WorkModes.ReportErrorsAndGenerateSuppresionFile}"
						   This mode combines the previous two: In this mode, Acuminator will report errors and generate suppression records in the Acuminator suppression file.

						   Acuminator provides two mechanisms to suppress its diagnostics:
						   - Local suppression with a suppression comment
						   - Global suppression with a suppression file

						   Each of the mechanisms is used for different scenarios. Local suppression provides a notice to the reader that there is an Acuminator alert suppressed due to the specified reasons.
						   Global suppression does not provide any information regarding alerts suppressed in the code or the reason for the suppression of the alert to the reader.
						   It is useful for development processes in large codebases where the number of errors is large and the cost of fixing them is high.
						   For example, you can use the Acuminator Console Runner in CI scenarios to run automatic tests with Acuminator static analysis
						   which will rely on Acuminator suppression file for the main code base and report Acuminator warnings and errors only for the new code.
						   
						   Acuminator work modes are designed to provide better support for automated CI scenarios.
						   In most cases, the regular analysis which simply outputs unsuppressed errors, is used. Therefore, it is a default work mode.
						   
						   The suppression file generator mode is designed to support CI scenarios by implementing automatic generation of the suppression file for a given codebase.
						   It does not output found errors. Instead, it generates suppression records for all errors that have been found in the code and adds them to the suppression file.
						   Note that it does not generate suppression records for errors that are already suppressed in the code with local suppression comments.
						   
						   The third work mode combines output of errors and suppression file generation. It can be used in scenarios like Acumatica tests
						   where the same integration test can be used both to find errors in the new code and generate new suppression file for a project in a convenient way from the UI.
						   This work mode allows to both enhance the outputted report with precise locations of found errors and keep generating the suppression file all in scope of one test. 
						   """)]
		public string? AcuminatorWorkMode { get; }

		/// <summary>
		/// The report grouping. By default, there is no grouping. You can make grouping by source file paths, diagnostic IDs, or both:<br/>
		///	- Add "<c>f</c>" or "<c>F</c>" to group found errors by source file.<br/>
		/// - Add "<c>d</c>" or "<c>D</c>" to group found errors by Acuminator diagnostic IDs.<br/><br/>
		///	Any combination of these characters will specify a report grouping. For example, specify "<c>fd</c>" to group errors in the report both by files and diagnostic IDs.
		/// </summary>
		/// <remarks>
		/// Reports grouping works like this:<br/>
		/// - First, errors in the report are grouped by filepaths, if "<c>f</c>" or "<c>F</c>" is specified in the grouping.<br/>
		/// - Second, errors in the report are grouped by diagnostic IDs, if "<c>d</c>" or "<c>D</c>" is specified in the grouping.<br/>
		/// </remarks>
		[Option(shortName: CommandLineArgNames.ReportGroupingShort, longName: CommandLineArgNames.ReportGroupingLong,
				HelpText = """
		Optional. The way the diagnostics are grouped in the report. By default, there is no grouping. You can group diagnostics by source file paths, diagnostic IDs, or both:
		  - Add "f" or "F" to group results by source file.
		  - Add "d" or "D" to group found errors by Acuminator diagnostic IDs.

		Any combination of these characters specifies the report grouping. For example, specify "fd" to group errors in the report by files and diagnostic IDs.
		
		Report grouping works like as follows:
		First, errors in the report are grouped by filepaths, if "f" or "F" is specified in the grouping.
		Then errors in the report are grouped by diagnostic IDs, if "d" or "D" is specified in the grouping.
		""")]
		public string? ReportGrouping { get; }

		/// <summary>
		/// When this optional flag is specified, the code analysis will report Acuminator informational diagnostics with severity <see cref="DiagnosticSeverity.Info"/>.<br/>
		/// By default, Acuminator does not report such diagnostics.
		/// </summary>
		[Option(longName: CommandLineArgNames.EnableInformationalDiagnostics, Default = false,
				HelpText = $"""
						   Optional. A flag that enables Acuminator informational diagnostics with severity "{nameof(DiagnosticSeverity.Info)}".
						   By default, Acuminator does not report such diagnostics.
						   """)]
		public bool EnableInformationalDiagnostics { get; }

		/// <summary>
		/// An optional flag which forces the Acuminator Console Runner to run in non-interactive mode.<br/>
		/// In this mode some interactive features like interactive cancellation of the analysis with "Ctrl + C" are disabled.<br/>
		/// This mode is useful when Acuminator is run in automated environments like CI/CD pipelines where no user interaction is possible.<br/>
		/// Some automated environments like Azure Pipelines may throw error if the application will attempt to use one of interactive features.
		/// By default, Acuminator Console Runner runs the analysis in the interactive mode.
		/// </summary>
		[Option(longName: CommandLineArgNames.UseNonInteractiveMode, Default = false,
				HelpText = $"""
						   Optional. A flag that forces the Acuminator Console Runner to run in non-interactive mode.
						   In this mode some interactive features like interactive cancellation of the analysis with "Ctrl + C" are disabled.
						   This mode is useful when Acuminator is run in automated environments like CI/CD pipelines where no user interaction is possible.
						   Some automated environments like Azure Pipelines may throw error if the application attempts to use one of interactive features.
						   By default, Acuminator Console Runner runs the analysis in the interactive mode.
						   """)]
		public bool UseNonInteractiveMode { get; }

		// Constructor arguments order must be the same as the properties order. This allows command line parser to initialize immutable options object via constructor.
		// See this for details: https://github.com/commandlineparser/commandline/wiki/Immutable-Options-Type
		public CommandLineOptions(string codeSource, string verbosity, bool disableSuppressionMechanism, string? msBuildPath, string? outputFileName, 
								  bool outputAbsolutePathsToUsages, string? outputFormat, bool isvSpecificAnalysisIsEnabled, 
								  bool px1007DiagnosticIsEnabled, bool disablePX1099Diagnostic, string? bannedApiFilePath, string? allowedApisFilePath,
								  string? acuminatorWorkMode, string? reportGrouping, bool enableInformationalDiagnostics, bool useNonInteractiveMode)
		{
			CodeSource 					   = codeSource;
			Verbosity 					   = verbosity;
			DisableSuppressionMechanism    = disableSuppressionMechanism;
			MSBuildPath 				   = msBuildPath;
			OutputFileName				   = outputFileName;
			OutputAbsolutePathsToUsages    = outputAbsolutePathsToUsages;
			OutputFormat				   = outputFormat;
			IsvSpecificAnalysisIsEnabled   = isvSpecificAnalysisIsEnabled;
			PX1007DiagnosticIsEnabled 	   = px1007DiagnosticIsEnabled;
			DisablePX1099Diagnostic		   = disablePX1099Diagnostic;
			BannedApiFilePath			   = bannedApiFilePath;
			AllowedApisFilePath			   = allowedApisFilePath;
			AcuminatorWorkMode 			   = acuminatorWorkMode;
			ReportGrouping				   = reportGrouping;
			EnableInformationalDiagnostics = enableInformationalDiagnostics;
			UseNonInteractiveMode		   = useNonInteractiveMode;
		}
	}
}