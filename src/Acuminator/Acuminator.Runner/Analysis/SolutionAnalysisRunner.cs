using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using Serilog;

using Acuminator.Runner.Input;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Analysis
{
	/// <summary>
	/// A solution analysis runner that does preparatory work - load solution for analysis and calls analyzer.
	/// </summary>
	[SuppressMessage("CodeQuality", "Serilog004:Constant MessageTemplate verifier", 
					 Justification = "Resource strings are used to simplify review by Doc Team")]
	internal class SolutionAnalysisRunner
	{
		private readonly ILogger _logger;

		public SolutionAnalysisRunner(ILogger logger)
		{
			_logger = logger.CheckIfNull();
		}

		public async Task<RunResult> RunAnalysisAsync(AnalysisContext analysisContext, CancellationToken cancellationToken)
		{
			analysisContext.ThrowOnNull();

			if (cancellationToken.IsCancellationRequested)
				return RunResult.Cancelled;

			RunResult runResult = RunResult.Success;
			bool hasErrors = false;

			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				runResult = await LoadAndAnalyzeCodeSourceAsync(analysisContext, cancellationToken);
			}
			catch (OperationCanceledException cancellationException)
			{
				_logger.Debug(Messages.ValidationWasSuccessfullyCancelledDebugInfo);
				runResult = RunResult.Cancelled;
			}
			catch (Exception exception)
			{
				_logger.Error(exception, Messages.AnalysisOfCodeSourceRuntimeError, analysisContext.CodeSource.Location);
				hasErrors = true;
			}

			return hasErrors
				? RunResult.RunTimeError
				: runResult;
		}

		private async Task<RunResult> LoadAndAnalyzeCodeSourceAsync(AnalysisContext analysisContext, CancellationToken cancellationToken)
		{
			_logger.Information(Messages.StartAnalyzingTheCodeSourceStatusMessage, analysisContext.CodeSource.Location);

			using var workspace = MSBuildWorkspace.Create();

			workspace.RegisterWorkspaceFailedHandler(OnCodeSourceLoadError);

			_logger.Information(Messages.StartLoadingTheCodeSourceAtPathStatusMessage, analysisContext.CodeSource.Location);
			var solution = await analysisContext.CodeSource.LoadSolutionAsync(workspace, cancellationToken)
														   .ConfigureAwait(false);
			if (solution == null)
			{
				_logger.Error(Messages.FailedToLoadSolutionFromCodeSourceError, analysisContext.CodeSource.Location);
				return RunResult.RunTimeError;
			}

			_logger.Information(Messages.SuccessfullyLoadedCodeSourceAtPathStatusMessage, analysisContext.CodeSource.Location);
			_logger.Debug(Messages.LoadedProjectsCount_Information, solution.ProjectIds.Count);

			_logger.Information(Messages.InitializeAcuminatorAnalyzersStatusMessage);
			var solutionValidator = AcuminatorAnalysisSolutionValidator.CreateSolutionValidator(analysisContext, _logger);

			if (solutionValidator == null)
				return RunResult.RunTimeError;

			_logger.Information(Messages.StartValidatingSolutionStatusMessage);

			RunResult validationResult = await solutionValidator.AnalyseSolution(solution, analysisContext, cancellationToken);

			_logger.Information(Messages.SuccessfullyFinishedSolutionValidationStatusMessage);
			return validationResult;
		}

		private void OnCodeSourceLoadError(WorkspaceDiagnosticEventArgs e)
		{
			switch (e.Diagnostic.Kind)
			{
				case WorkspaceDiagnosticKind.Failure:
					_logger.Error("{WorkspaceDiagnostic}", e.Diagnostic);
					break;
				case WorkspaceDiagnosticKind.Warning:
					_logger.Warning("{WorkspaceDiagnostic}", e.Diagnostic);
					break;
			}
		}
	}
}