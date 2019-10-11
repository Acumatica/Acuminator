using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;

namespace Acuminator.Utilities.DiagnosticSuppression.CodeActions
{
	/// <summary>
	/// A code action operation to load new suppression file by suppression manager after it was added to workspace.
	/// </summary>
	internal class LoadNewSuppressionFileOperation : CodeActionOperation
	{
		private readonly string _filePath;
		private readonly Project _project;
		private readonly Diagnostic _diagnosticToSuppress;
		private readonly SemanticModel _semanticModel;

		public override string Title => "Load new suppression file and suppress diagnostic code action operation";

		public LoadNewSuppressionFileOperation(string filePath, Diagnostic diagnosticToSuppress,
																			  Project project, SemanticModel semanticModel)
		{
			_filePath = filePath;
			_project = project.CheckIfNull(nameof(project));
			_diagnosticToSuppress = diagnosticToSuppress.CheckIfNull(nameof(diagnosticToSuppress));
			_semanticModel = semanticModel.CheckIfNull(nameof(semanticModel));
		}

		public override void Apply(Workspace workspace, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			SuppressionFile suppressionFile = _filePath.IsNullOrWhiteSpace()
				? null
				: SuppressionManager.Instance?.LoadSuppressionFileFrom(_filePath);

			if (suppressionFile == null)
			{
				ShowErrorForFileNotFoundMessage();
				return;
			}

			if (!SuppressionManager.SuppressDiagnostic(_semanticModel, _diagnosticToSuppress.Id, _diagnosticToSuppress.Location.SourceSpan,
														_diagnosticToSuppress.DefaultSeverity, cancellationToken))
			{
				ShowErrorForSuppressionNotAddedMessage();
			}
		}

		private void ShowErrorForFileNotFoundMessage()
		{
			var errorMessage = new LocalizableResourceString(nameof(Resources.DiagnosticSuppression_FailedToFindSuppressionFile),
																    Resources.ResourceManager, typeof(Resources), _project.Name);
			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage.ToString()}");
		}

		private void ShowErrorForSuppressionNotAddedMessage()
		{
			var errorMessage = new LocalizableResourceString(nameof(Resources.DiagnosticSuppression_FailedToAddToSuppressionFile),
															 Resources.ResourceManager, typeof(Resources), _filePath);
			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage.ToString()}");
		}
	}
}
