using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Runner.Constants;
using Acuminator.Runner.Input;
using Acuminator.Runner.Resources;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;

using Microsoft.CodeAnalysis;

using Serilog;

using AnalyzerFileReference = Microsoft.CodeAnalysis.Diagnostics.AnalyzerFileReference;
using DiagnosticAnalyzer = Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer;

using static Acuminator.Runner.Constants.CommandLineArgNames.WorkModes;

namespace Acuminator.Runner.Analysis.Initialization
{
	internal class AcuminatorAnalysisInitializer
	{
		private readonly AnalysisContext _analysisContext;
		private readonly ILogger _logger;

		public AcuminatorAnalysisInitializer(AnalysisContext analysisContext, ILogger logger)
		{
			_analysisContext = analysisContext.CheckIfNull();
			_logger = logger.CheckIfNull();
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", 
						 Justification = "Resource strings are used to simplify review by Doc Team")]
		public (bool AreSettingsInitialized, ImmutableArray<DiagnosticAnalyzer> Analyzers) InitializeAcuminatorSettingsAndGetAnalyzers()
		{
			try
			{
				GlobalSettings.InitializeGlobalSettingsOnce(_analysisContext.CodeAnalysisSettings, _analysisContext.BannedApiSettings, AnalysisHostType.Runner);
				var analyzers = CollectAnalyzers();

				var acuminatorVersion = typeof(Acuminator.SharedConstants).Assembly.GetName()?.Version;

				if (acuminatorVersion != null)
					_logger.Information(Messages.AcuminatorVersionInfo, acuminatorVersion);
				else
					_logger.Warning(Messages.FailedToObtainAcuminatorVersionWarning);

				return (AreSettingsInitialized: true, Analyzers: analyzers);
			}
			catch (Exception e)
			{
				_logger.Error(e, Messages.ErrorDuringAcuminatorAnalyzersCollection);
				return (AreSettingsInitialized: false, Analyzers: []);
			}
		}

		private ImmutableArray<DiagnosticAnalyzer> CollectAnalyzers()
		{
			var acuminatorAnalyzersPath = typeof(PXDiagnosticAnalyzer).Assembly.Location;
			var analyzerReference = new AnalyzerFileReference(acuminatorAnalyzersPath, new AnalyzerAssemblyLoader());
			var analyzers = analyzerReference.GetAnalyzers(LanguageNames.CSharp);

			return analyzers;
		}

		public bool InitializeAcuminatorGlobalSuppressionMechanismForCodeSource(Solution solution)
		{
			solution.ThrowOnNull();

			switch (_analysisContext.CodeSource.Type)
			{
				case CodeSources.CodeSourceType.Project:
					var project = _analysisContext.CodeSource.GetProjectsForValidation(solution).FirstOrDefault();

					return project != null
						? InitializeAcuminatorGlobalSuppressionMechanismForProject(project)
						: true;

				case CodeSources.CodeSourceType.Solution:
					return InitializeAcuminatorGlobalSuppressionMechanismForSolution(solution);

				default:
					if (_analysisContext.WorkMode.HasFlag(AcuminatorWorkMode.GenerateSuppressionFile))
					{
						_logger.Error("""
									  The Acuminator console tool is configured to generate a suppression file for the code source "{CodeSource}" but the code source is not a project or solution.
									  The generation of Acuminator suppression file is supported only for a project or solution.
									  """,
									 _analysisContext.CodeSource.Location);
						return false;
					}
					else
					{
						_logger.Warning("Acuminator suppression via suppression file is not supported for the code source \"{CodeSource}\". The code source is not a project or solution.",
										_analysisContext.CodeSource.Location);
						return true;
					}
			}
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "Ok to use a string variable due to a long message")]
		private bool InitializeAcuminatorGlobalSuppressionMechanismForProject(Project project)
		{
			var acuminatorSuppressionFiles = project.AdditionalDocuments
													.Where(d => d.FilePath.IsSuppressionFile(checkFileExists: true))
													.Select(d => new GlobalSuppressionFileInitInfo(d.FilePath!, _analysisContext.WorkMode))
													.ToList(capacity: 1);

			if (_analysisContext.WorkMode.HasFlag(AcuminatorWorkMode.GenerateSuppressionFile) && acuminatorSuppressionFiles.Count == 0)
			{
				string errorMsg = CreateErrorMessage();
				_logger.Error(errorMsg);
				return false;
			}

			SuppressionManager.InitOrReset(acuminatorSuppressionFiles,
										   fileSystemServiceFabric: () => new SuppressionFileSystemServiceForConsoleRunner(
																				new ConsoleRunnerIOErrorObserver(_logger)));
			return true;

			//-----------------------------------------Local Function-----------------------------------------------------
			string CreateErrorMessage()
			{
				string suppressionFileName = project.Name + SuppressionFile.SuppressionFileExtension;
				return $"""
					   Acuminator Console Runner is configured to generate a suppression file for the "{project.Name}" project 
					   because the "--{CommandLineArgNames.AcuminatorWorkModeLong}" option was set to either "{GenerateSuppressionFile}" or "{ReportErrorsAndGenerateSuppresionFile}".
					   However, the project does not contain an Acuminator suppression file.
					   To generate the project's suppression file, in the project's root folder, you need to create an empty suppression file with the "{suppressionFileName}" name.
					   The content of the empty suppression file should contain the following code.

					   <?xml version="1.0" encoding="utf-8"?>
					   <suppressions>
					   </suppressions>

					   You should also specify the suppression file name in the "AdditionalFiles" section of the "{project.FilePath}" project file as shown below.

					   <ItemGroup>
					     <AdditionalFiles Include="{suppressionFileName}"/>
					   </ItemGroup>
					   """;
			}
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "Ok to use a string variable due to a long message")]
		private bool InitializeAcuminatorGlobalSuppressionMechanismForSolution(Solution solution)
		{
			var acuminatorSuppressionFiles = solution.Projects
													 .SelectMany(project => project.AdditionalDocuments)
													 .Where(d => d.FilePath.IsSuppressionFile(checkFileExists: true))
													 .Select(d => new GlobalSuppressionFileInitInfo(d.FilePath!, _analysisContext.WorkMode))
													 .ToList(capacity: 1);

			if (_analysisContext.WorkMode.HasFlag(AcuminatorWorkMode.GenerateSuppressionFile) && acuminatorSuppressionFiles.Count == 0)
			{
				string errorMsg = CreateErrorMessage();
				_logger.Error(errorMsg);
				return false;
			}

			SuppressionManager.InitOrReset(acuminatorSuppressionFiles,
										   fileSystemServiceFabric: () => new SuppressionFileSystemServiceForConsoleRunner(
																				new ConsoleRunnerIOErrorObserver(_logger)));
			return true;

			//-----------------------------------------Local Function-----------------------------------------------------
			string CreateErrorMessage()
			{
				return $"""
					   Acuminator Console Runner is configured to generate a suppression file for projects of the "{solution.FilePath}" solution 
					   because the "--{CommandLineArgNames.AcuminatorWorkModeLong}" option was set to either "{GenerateSuppressionFile}" or "{ReportErrorsAndGenerateSuppresionFile}".
					   However, the solution does not contain any project with an Acuminator suppression file.
					   To generate a suppression file for a project, in the project's root folder, you need to create an empty suppression file for the project with the name "<ProjectName>.{SuppressionFile.SuppressionFileExtension}".
					   The content of the empty suppression file should contain the following code.

					   <?xml version="1.0" encoding="utf-8"?>
					   <suppressions>
					   </suppressions>

					   You should also specify the suppression file name in the "AdditionalFiles" section of the project .csproj file as shown below.

					   <ItemGroup>
					     <AdditionalFiles Include="<ProjectName>.{SuppressionFile.SuppressionFileExtension}"/>
					   </ItemGroup>
					   """;
			}
		}
	}
}
