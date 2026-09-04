using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using Serilog;

using Acuminator.Runner.Input;
using Acuminator.Runner.Resources;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Analysis
{
	/// <summary>
	/// A solution analysis runner that does preparatory work - register MSBuild, load solution for analysis and calls analyzer.
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
			else if (!TryRegisterMSBuild(analysisContext))
				return RunResult.RunTimeError;

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
			finally
			{
				if (!TryUnregisterMSBuild())
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

			try
			{
				workspace.WorkspaceFailed += OnCodeSourceLoadError;

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
			finally
			{
				workspace.WorkspaceFailed -= OnCodeSourceLoadError;
			}
		}

		private void OnCodeSourceLoadError(object sender, WorkspaceDiagnosticEventArgs e)
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

		private bool TryRegisterMSBuild(AnalysisContext analysisContext)
		{
			if (MSBuildLocator.IsRegistered)
				return true;

			if (!MSBuildLocator.CanRegister)
			{
				_logger.Warning(Messages.MSBuild_RegistrationDeniedWarning);
				return false;
			}

			if (analysisContext.MSBuildPath != null)
			{
				return TryRegisterMSBuildByPath(analysisContext.MSBuildPath);
			}

			_logger.Information(Messages.SearchingForMSBuildInstancesStatusMessage);

			var vsInstances = MSBuildLocator.QueryVisualStudioInstances();
			VisualStudioInstance? latestVSInstance = vsInstances.OrderByDescending(vsInstance => vsInstance.Version)
																.FirstOrDefault();
			if (latestVSInstance == null)
			{
				_logger.Error(Messages.NoInstalledMSBuildFoundError);
				return false;
			}

			_logger.Information(Messages.MSBuild_VisualStudioNameAndVersion_Info, latestVSInstance.Name, latestVSInstance.Version);
			_logger.Information(Messages.MSBuildPath_Info, latestVSInstance.MSBuildPath);

			try
			{
				MSBuildLocator.RegisterInstance(latestVSInstance);
				return true;
			}
			catch (Exception e)
			{
				_logger.Error(e, Messages.MSBuildInstanceRegistrationError);
				return false;
			}
		}

		private bool TryRegisterMSBuildByPath(string msBuildPath)
		{
			bool fileExists = File.Exists(msBuildPath);
			bool directoryExists = Directory.Exists(msBuildPath);

			if (!fileExists && !directoryExists)
			{
				_logger.Error(Messages.MSBuildDoesNotExistAtTheProvidedPathError, msBuildPath);
				return false;
			}

			string? msBuildDir;
			
			if (fileExists)
			{
				string expandedFileName = Path.GetFullPath(msBuildPath);
				msBuildDir = Path.GetDirectoryName(expandedFileName);
			}
			else
				msBuildDir = msBuildPath;

			_logger.Information(Messages.RegisteringMSBuildAtTheProvidedPathStatusMessage, msBuildDir);

			try
			{
				
				MSBuildLocator.RegisterMSBuildPath(msBuildDir);

				_logger.Information(Messages.SuccessfullyRegisteredMSBuildAtProvidedPathStatusMessage, msBuildDir);
				return true;
			}
			catch (Exception e)
			{
				_logger.Error(e, Messages.MSBuildRegistrationAtProvidedPathFailedError, msBuildDir);
				return false;
			}
		}

		private bool TryUnregisterMSBuild()
		{
			try
			{
				MSBuildLocator.Unregister();
				return true;
			}
			catch (Exception e)
			{
				_logger.Error(e, Messages.UnregisterMSBuildInstanceError);
				return false;
			}
		}
	}
}