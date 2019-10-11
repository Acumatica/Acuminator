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
	/// A code action operation to suppress diagnostic in the suppression file.
	/// </summary>
	internal class SuppressInSuppressionFileOperation : CodeActionOperation
	{
		private readonly string _projectName;
		private readonly Diagnostic _diagnosticToSuppress;
		private readonly SemanticModel _semanticModel;

		public override string Title => "Suppress diagnostic code action in the suppression file operation";

		public SuppressInSuppressionFileOperation(Diagnostic diagnosticToSuppress, string projectName, SemanticModel semanticModel)
		{
			_projectName = projectName.CheckIfNullOrWhiteSpace(nameof(projectName));
			_diagnosticToSuppress = diagnosticToSuppress.CheckIfNull(nameof(diagnosticToSuppress));
			_semanticModel = semanticModel.CheckIfNull(nameof(semanticModel));
		}

		public override void Apply(Workspace workspace, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SuppressionFile suppressionFile = SuppressionManager.Instance.GetSuppressionFile(_projectName);

			if (suppressionFile == null)
			{
				ShowErrorForFileNotFoundMessage();
				return;
			}

			cancellationToken.ThrowIfCancellationRequested();

			if (!SuppressionManager.SuppressDiagnostic(_semanticModel, _diagnosticToSuppress.Id, _diagnosticToSuppress.Location.SourceSpan,
														_diagnosticToSuppress.DefaultSeverity, cancellationToken))
			{
				ShowErrorForSuppressionNotAddedMessage(suppressionFile.Path);
			}
		}

		private void ShowErrorForFileNotFoundMessage()
		{
			var errorMessage = new LocalizableResourceString(nameof(Resources.DiagnosticSuppression_FailedToFindSuppressionFile),
																    Resources.ResourceManager, typeof(Resources), _projectName);
			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage.ToString()}");
		}

		private void ShowErrorForSuppressionNotAddedMessage(string filePath)
		{
			var errorMessage = new LocalizableResourceString(nameof(Resources.DiagnosticSuppression_FailedToAddToSuppressionFile),
															 Resources.ResourceManager, typeof(Resources), filePath);
			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage.ToString()}");
		}
	}
}
