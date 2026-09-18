using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Runner.Analysis.Initialization;
using Acuminator.Runner.Output;
using Acuminator.Runner.Output.Data;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Serilog;

namespace Acuminator.Runner.Analysis
{
	internal sealed class AcuminatorAnalysisSolutionValidator
	{
		private readonly IOutputterFactory _outputterFactory;
		private readonly IProjectReportBuilder _reportBuilder;
		private readonly ILogger _logger;
		private readonly AcuminatorAnalysisInitializer _acuminatorAnalysisInitializer;

		private readonly ImmutableArray<DiagnosticAnalyzer> _diagnosticAnalyzers;

		private AcuminatorAnalysisSolutionValidator(ImmutableArray<DiagnosticAnalyzer> diagnosticAnalyzers, ILogger logger,
													AcuminatorAnalysisInitializer acuminatorAnalysisInitializer,
													IProjectReportBuilder? customReportBuilder = null,
													IOutputterFactory? customOutputFactory = null)
		{
			_diagnosticAnalyzers 		   = diagnosticAnalyzers;
			_logger 					   = logger;
			_acuminatorAnalysisInitializer = acuminatorAnalysisInitializer;
			_reportBuilder 				   = customReportBuilder ?? new ProjectReportBuilder();
			_outputterFactory 			   = customOutputFactory ?? new ReportOutputterFactory();
		}

		public static AcuminatorAnalysisSolutionValidator? CreateSolutionValidator(Input.AnalysisContext analysisContext, ILogger logger)
		{
			var acuminatorAnalysisInitializer = new AcuminatorAnalysisInitializer(analysisContext, logger);
			var (areSettingsInitialized, diagnosticAnalyzers) = acuminatorAnalysisInitializer.InitializeAcuminatorSettingsAndGetAnalyzers();

			return areSettingsInitialized && !diagnosticAnalyzers.IsDefaultOrEmpty
					? new AcuminatorAnalysisSolutionValidator(diagnosticAnalyzers, logger, acuminatorAnalysisInitializer)
					: null;
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier",
						 Justification = "Resource strings are used to simplify review by Doc Team")]
		public async Task<RunResult> AnalyseSolution(Solution solution, Input.AnalysisContext analysisContext, CancellationToken cancellationToken)
		{
			if (_diagnosticAnalyzers.IsDefaultOrEmpty)
			{
				_logger.Error(Messages.FailedToLoadAcuminatorAnalyzersError, analysisContext.CodeSource.Location);
				return RunResult.RunTimeError;
			}

			if (!_acuminatorAnalysisInitializer.InitializeAcuminatorGlobalSuppressionMechanismForCodeSource(solution))
			{
				_logger.Error(Messages.FailedToInitializeAcuminatorGlobalSuppressionMechanismError, analysisContext.CodeSource.Location);
				return RunResult.RunTimeError;
			}

			RunResult solutionValidationResult = RunResult.Success;
			var projectsToValidate = analysisContext.CodeSource.GetProjectsForValidation(solution)
															   .OrderBy(p => p.Name);

			using (var reportOutputter = _outputterFactory.CreateOutputter(analysisContext))
			{
				var projectReports = new List<ProjectReport>(capacity: solution.ProjectIds.Count);
				bool hasRefenceToAcumatica = false;

				foreach (Project project in projectsToValidate)
				{
					_logger.Information(Messages.StartedAcuminatorValidationOfTheProjectInfo, project.Name);

					if (cancellationToken.IsCancellationRequested)
					{
						_logger.Information(Messages.CancelledCodeSourceValidationInfo, analysisContext.CodeSource.Location, project.Name, RunResult.Cancelled);
						solutionValidationResult = solutionValidationResult.Combine(RunResult.Cancelled);
						return solutionValidationResult;
					}

					var (projectValidationResult, projectReport, isPlatformReferenced) =
						await AnalyzeProject(project, analysisContext, cancellationToken).ConfigureAwait(false);

					hasRefenceToAcumatica = hasRefenceToAcumatica || isPlatformReferenced;

					if (projectReport != null)
						projectReports.Add(projectReport);

					solutionValidationResult = solutionValidationResult.Combine(projectValidationResult);

					_logger.Information(Messages.FinishedAcuminatorValidationOfTheProjectInfo, project.Name, projectValidationResult);
				}

				if (!hasRefenceToAcumatica)
				{
					_logger.Error(Messages.NoProjectInCodeSourceReferencesAcumaticaPlatformError, analysisContext.CodeSource.Location);
					return RunResult.RunTimeError;
				}

				if (analysisContext.WorkMode.HasFlag(AcuminatorWorkMode.GenerateSuppressionFile))
				{
					SuppressionManager.SaveSuppressionFiles(saveOnlyGeneratedFiles: true);
				}

				if (projectReports.Count > 0)
				{
					var codeSourceReport = new CodeSourceReport(analysisContext.CodeSource.Location, projectReports);
					reportOutputter.OutputReport(codeSourceReport, analysisContext, cancellationToken);
				}
			}

			return solutionValidationResult;
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier",
						 Justification = "Resource strings are used to simplify review by Doc Team")]
		private async Task<(RunResult ValidationResult, ProjectReport? Report, bool IsPlatformReferenced)> AnalyzeProject(Project project,
																												Input.AnalysisContext analysisContext,
																												CancellationToken cancellationToken)
		{
			_logger.Debug(Messages.ObtainingRoslynCompilationDataForTheProjectDebug, project.Name);
			var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

			if (compilation == null)
			{
				_logger.Error(Messages.FailedToObtainRoslynCompilationDataForTheProjectError, project.Name, project.FilePath);
				return (RunResult.RunTimeError, Report: null, IsPlatformReferenced: false);
			}

			_logger.Debug(Messages.ObtainedRoslynCompilationDataForTheProjectDebug, project.Name);

			if (!IsPlatformReferenced(compilation))
			{
				if (analysisContext.CodeSource.Type == CodeSources.CodeSourceType.Project)
				{
					_logger.Error(Messages.ProjectDoesNotReferenceAcumaticaPlatformValidationError, project.Name);
					return (RunResult.RequirementsNotMet, Report: null, IsPlatformReferenced: false);
				}
				else
				{
					// For solution with multiple projects we will not fail only if there are no projects referencing Acumatica Platform.
					_logger.Warning(Messages.ProjectDoesNotReferenceAcumaticaPlatformValidationError, project.Name);
					return (RunResult.Success, Report: null, IsPlatformReferenced: false);
				}
			}

			var (validationResult, projectReport) = await RunAnalyzersOnProjectAsync(compilation, analysisContext, project, cancellationToken)
															.ConfigureAwait(false);
			return (validationResult, projectReport, IsPlatformReferenced: true);
		}

		private bool IsPlatformReferenced(Compilation compilation)
		{
			var acuminatorPxContext = new PXContext(compilation, codeAnalysisSettings: null);
			return acuminatorPxContext.IsPlatformReferenced;
		}

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier",
						 Justification = "Resource strings are used to simplify review by Doc Team")]
		private async Task<(RunResult ValidationResult, ProjectReport? Report)> RunAnalyzersOnProjectAsync(Compilation compilation,
																										   Input.AnalysisContext analysisContext,
																										   Project project, CancellationToken cancellation)
		{
			var analyzerOptions = GetAnalyzerOptions();
			var compilationAnalysisOptions = new CompilationWithAnalyzersOptions(analyzerOptions!, OnAnalyzerException,
																				 concurrentAnalysis: !Debugger.IsAttached,
																				 logAnalyzerExecutionTime: false);
			var compilationWithAnalyzers = new CompilationWithAnalyzers(compilation, _diagnosticAnalyzers, compilationAnalysisOptions);

			var diagnosticResults = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellation).ConfigureAwait(false);
			var filteredResults = FilterDiagnosticResults(diagnosticResults, analysisContext);

			if (filteredResults.IsDefaultOrEmpty)
			{
				_logger.Information(Messages.AcuminatorValidationPassedMessage, project.Name);
				var successfulReport = ProjectReport.SuccessfulReport(project.Name);
				return (RunResult.Success, successfulReport);
			}
			else
				_logger.Error($"{{Project}} - {Messages.ErrorsCountReportTitlePart}: {{ErrorCount}}", project.Name, filteredResults.Length);

			ProjectReport projectReport = _reportBuilder.BuildReport(filteredResults, analysisContext, project, cancellation);

			return (RunResult.RequirementsNotMet, projectReport);
		}

		private AnalyzerOptions? GetAnalyzerOptions() => null;      // TODO Here we may need to put support for .editorcofing files in the future

		[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", Justification = "Ok to use runtime dependent new line in message")]
		private void OnAnalyzerException(Exception exception, DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
		{
			var prettyLocation = diagnostic.Location.GetMappedLineSpan().ToString();

			string errorMsg = $"Analyzer error:{Environment.NewLine}{{Id}}{Environment.NewLine}{{Location}}{Environment.NewLine}{{Analyzer}}";
			_logger.Error(exception, errorMsg, diagnostic.Id, prettyLocation, analyzer.ToString());
		}

		private ImmutableArray<Diagnostic> FilterDiagnosticResults(ImmutableArray<Diagnostic> diagnosticResults, Input.AnalysisContext analysisContext)
		{
			if (diagnosticResults.IsDefaultOrEmpty)
				return diagnosticResults;

			var filteredDiagnostics = diagnosticResults.Where(d => d.IsAcuminatorDiagnostic());

			if (analysisContext.CodeAnalysisSettings.SuppressionMechanismEnabled)
				filteredDiagnostics = filteredDiagnostics.Where(d => !d.IsSuppressed);

			return filteredDiagnostics.ToImmutableArray();
		}
	}
}