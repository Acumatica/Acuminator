using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;

using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;
using Document = Microsoft.CodeAnalysis.Document;
using Shell = Microsoft.VisualStudio.Shell;
using Resources = Acuminator.Utilities.Resources;

namespace Acuminator.Vsix.DiagnosticSuppression
{
	/// <summary>
	/// Suppress Diagnostic Command.
	/// </summary>
	internal sealed class SuppressDiagnosticInSuppressionFileCommand : SuppressDiagnosticCommandBase
	{
		private static int _isCommandInitialized = NOT_INITIALIZED;

		/// <summary>
		/// Suppress Diagnostic command ID.
		/// </summary>
		public const int SuppressDiagnosticInSuppressionFileId = 0x0201;

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static SuppressDiagnosticInSuppressionFileCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SuppressDiagnosticInSuppressionFileCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private SuppressDiagnosticInSuppressionFileCommand(Shell.AsyncPackage package, Shell.OleMenuCommandService commandService) : 
									 base(package, commandService, SuppressDiagnosticInSuppressionFileId)
		{
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">The command service.</param>
		public static void Initialize(Shell.AsyncPackage package, Shell.OleMenuCommandService commandService)
		{
			if (Interlocked.CompareExchange(ref _isCommandInitialized, value: INITIALIZED, comparand: NOT_INITIALIZED) == NOT_INITIALIZED)
			{
				Instance = new SuppressDiagnosticInSuppressionFileCommand(package, commandService);
			}
		}
		protected override async Task SuppressSingleDiagnosticOnNodeAsync(DiagnosticData diagnostic, Document document, SyntaxNode syntaxRoot,
																	SemanticModel semanticModel, SyntaxNode nodeWithDiagnostic)
		{
			if (diagnostic?.DataLocation?.SourceSpan == null)
				return;

			var (suppressionFileRoslynDoc, project) = await GetProjectAndSuppressionFileAsync(diagnostic.ProjectId);
			bool suppressionFileExists = suppressionFileRoslynDoc != null;

			if (!suppressionFileExists)
			{
				if (project == null)
					return;

				SuppressionFile suppressionFile = SuppressionManager.CreateSuppressionFileForProject(project);
				suppressionFileExists = suppressionFile != null;
			}

			if (!suppressionFileExists || 
				!SuppressionManager.SuppressDiagnostic(semanticModel, diagnostic.Id, diagnostic.DataLocation.SourceSpan.Value,
													   diagnostic.DefaultSeverity, Package.DisposalToken))
			{
				ShowErrorMessage(suppressionFileRoslynDoc, project);
			}
		}

		private async Task<(TextDocument SuppressionFile, Project Project)> GetProjectAndSuppressionFileAsync(ProjectId projectId)
		{
			await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			var workspace = await Package.GetVSWorkspaceAsync();
			Project project = workspace?.CurrentSolution?.GetProject(projectId);

			if (project?.Name == null)
				return default;

			string suppressionFileName = project.Name + SuppressionFile.SuppressionFileExtension;
			TextDocument suppressionFile = project.AdditionalDocuments.FirstOrDefault(d => string.Equals(suppressionFileName, d.Name,
																						   StringComparison.OrdinalIgnoreCase));
			return (suppressionFile, project);
		}

		private void ShowErrorMessage(TextDocument suppressionFile, Project project)
		{
			LocalizableResourceString errorMessage;

			if (suppressionFile?.FilePath != null)
			{
				errorMessage = new LocalizableResourceString(nameof(Resources.DiagnosticSuppression_FailedToAddToSuppressionFile),
															 Resources.ResourceManager, typeof(Resources), suppressionFile.FilePath);
			}
			else
			{
				errorMessage = new LocalizableResourceString(nameof(Resources.DiagnosticSuppression_FailedToFindSuppressionFile),
															 Resources.ResourceManager, typeof(Resources), project.Name);
			}

			MessageBox.Show(errorMessage.ToString(), AcuminatorVSPackage.PackageName);
		}

		protected override Task SupressMultipleDiagnosticOnNodeAsync(List<DiagnosticData> diagnosticData, Document document, SyntaxNode syntaxRoot,
																	 SemanticModel semanticModel, SyntaxNode nodeWithDiagnostic)
		{
			MessageBox.Show(VSIXResource.DiagnosticSuppression_MultipleDiagnosticFound, AcuminatorVSPackage.PackageName);
			return Task.CompletedTask;
		}
	}
}
