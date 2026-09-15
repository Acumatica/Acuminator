using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

using Acuminator.Runner.Analysis.CodeSources;
using Acuminator.Runner.Constants;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Output.Grouping;
using Acuminator.Runner.Resources;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;

namespace Acuminator.Runner.Input
{
    internal class AnalysisContextBuilder
	{
		public AnalysisContext CreateContext(CommandLineOptions commandLineOptions)
		{
			commandLineOptions.ThrowOnNull();

			var codeSource = ReadCodeSource(commandLineOptions.CodeSource) ?? 
							 throw new ArgumentException(Messages.CodeSourceNotSpecifiedError);

			var codeAnalysisSettings = new CodeAnalysisSettings(CodeAnalysisSettings.DefaultRecursiveAnalysisEnabled,
																commandLineOptions.IsvSpecificAnalysisIsEnabled,
																staticAnalysisEnabled: true,                   // no sense to run the tool with disabled static analysis
																suppressionMechanismEnabled: !commandLineOptions.DisableSuppressionMechanism,
																commandLineOptions.PX1007DiagnosticIsEnabled,
																commandLineOptions.EnableInformationalDiagnostics);

			var bannedApiSettings = new BannedApiSettings(bannedApiAnalysisEnabled: !commandLineOptions.DisablePX1099Diagnostic,
														  commandLineOptions.BannedApiFilePath, 
														  commandLineOptions.AllowedApisFilePath);

			OutputFormat outputFormat   = GetOutputFormat(commandLineOptions.OutputFormat.NullIfWhiteSpace());
			GroupingMode groupingMode   = GetGroupingMode(commandLineOptions.ReportGrouping);
			AcuminatorWorkMode workMode = GetAcuminatorWorkMode(commandLineOptions.AcuminatorWorkMode);
			var input = new AnalysisContext(codeSource, codeAnalysisSettings, bannedApiSettings, commandLineOptions.OutputFileName,
											commandLineOptions.OutputAbsolutePathsToUsages, outputFormat, workMode, groupingMode);
			return input;
		}

		private ICodeSource? ReadCodeSource(string codeSourceLocation)
		{
			if (codeSourceLocation.IsNullOrWhiteSpace())
				return null;

			if (!File.Exists(codeSourceLocation))
			{
				string errorMessage = string.Format(CultureInfo.InvariantCulture, Messages.CodeSourceNotFoundError, codeSourceLocation);
				throw new ArgumentException(errorMessage);
			}

			string fullPath = Path.GetFullPath(codeSourceLocation);
			ICodeSource codeSource = CreateCodeSource(fullPath);
			return codeSource;
		}

		[SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "The message is supposed to be localized with a current culture")]
		private ICodeSource CreateCodeSource(string codeSourceLocation)
		{
			string extension = Path.GetExtension(codeSourceLocation);

			return extension switch
			{
				Constant.Common.ProjectFileExtension  => new ProjectCodeSource(codeSourceLocation),
				Constant.Common.SolutionFileExtension => new SolutionCodeSource(codeSourceLocation),
				_									  => throw new NotSupportedException(
															string.Format(Messages.NotSupportedCodeSourceType, codeSourceLocation))
			};
		}

		private OutputFormat GetOutputFormat(string? rawOutputFormat)
		{
			const string plainTextFormat = "text";
			const string jsonFormat = "json";

			if (rawOutputFormat == null || plainTextFormat.Equals(rawOutputFormat, StringComparison.OrdinalIgnoreCase))
				return OutputFormat.PlainText;
			else if (jsonFormat.Equals(rawOutputFormat, StringComparison.OrdinalIgnoreCase))
				return OutputFormat.Json;
			else
			{
				string errorMsg = string.Format(CultureInfo.CurrentCulture, Messages.NotSupportedOutputFormat, rawOutputFormat, plainTextFormat, jsonFormat);
				throw new NotSupportedException(errorMsg);
			}
		}

		private GroupingMode GetGroupingMode(string? rawGroupingLocation)
		{
			if (rawGroupingLocation.IsNullOrWhiteSpace())
				return GroupingMode.None;

			const char FileGroupingChar = 'F';
			const char DiagnosticGroupingChar = 'D';

			string rawGroupingLocationUpperCased = rawGroupingLocation.ToUpperInvariant();

			bool groupByFiles = rawGroupingLocationUpperCased.Contains(FileGroupingChar);
			bool groupByDiagnostic = rawGroupingLocationUpperCased.Contains(DiagnosticGroupingChar);

			GroupingMode grouping = groupByFiles
				? GroupingMode.Files
				: GroupingMode.None;

			if (groupByDiagnostic)
				grouping |= GroupingMode.DiagnosticIDs;

			return grouping;
		}

		private AcuminatorWorkMode GetAcuminatorWorkMode(string? rawWorkMode) => rawWorkMode switch
		{
			CommandLineArgNames.WorkModes.ReportErrors 							=> AcuminatorWorkMode.ReportUnsuppressedErrors,
			CommandLineArgNames.WorkModes.GenerateSuppressionFile 				=> AcuminatorWorkMode.GenerateSuppressionFile,
			CommandLineArgNames.WorkModes.ReportErrorsAndGenerateSuppresionFile => AcuminatorWorkMode.BothReportAndGenerate,
			_ 																	=> AcuminatorWorkMode.ReportUnsuppressedErrors
		};
	}
}