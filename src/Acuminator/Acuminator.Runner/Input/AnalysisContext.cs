using System;
using System.Runtime.InteropServices;

using Acuminator.Runner.Analysis.CodeSources;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Output.Grouping;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;

namespace Acuminator.Runner.Input
{
    internal class AnalysisContext
	{
		/// <summary>
		/// Gets the code source to validate.
		/// </summary>
		/// <value>
		/// The code source to validate.
		/// </value>
		public ICodeSource CodeSource { get; }

		public CodeAnalysisSettings CodeAnalysisSettings { get; }

		public BannedApiSettings BannedApiSettings { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputFileName"/>
		public string? OutputFileName { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputAbsolutePathsToUsages"/>
		public bool OutputAbsolutePathsToUsages { get; }

		/// <inheritdoc cref="CommandLineOptions.OutputFormat"/>
		public OutputFormat OutputFormat { get; }

		/// <summary>
		/// The Acuminator work mode.
		/// </summary>
		public AcuminatorWorkMode WorkMode { get; }

		/// <summary>
		/// The grouping mode for Acuminator errors in the report.
		/// </summary>
		public GroupingMode GroupingMode { get; }

		/// <summary>
		/// Are file paths on the underlying OS case sensitive or not.
		/// </summary>
		public bool CaseSensitiveFilePaths { get; }

		public AnalysisContext(ICodeSource codeSource, CodeAnalysisSettings codeAnalysisSettings, BannedApiSettings bannedApiSettings, 
							   string? outputFileName, bool outputAbsolutePathsToUsages, OutputFormat outputFormat,
							   AcuminatorWorkMode workMode, GroupingMode groupingMode)
		{
			CodeSource 					   = codeSource.CheckIfNull();
			CodeAnalysisSettings		   = codeAnalysisSettings.CheckIfNull();
			BannedApiSettings			   = bannedApiSettings.CheckIfNull();
			OutputFileName				   = outputFileName.NullIfWhiteSpace();
			OutputAbsolutePathsToUsages    = outputAbsolutePathsToUsages;
			OutputFormat				   = outputFormat;
			WorkMode					   = workMode;
			GroupingMode				   = groupingMode;
			CaseSensitiveFilePaths		   = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		}
	}
}