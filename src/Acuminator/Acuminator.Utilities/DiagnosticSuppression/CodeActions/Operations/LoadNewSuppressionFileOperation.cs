using System;
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
		private readonly string _projectName;

		public override string Title => "Load new suppression file code action operation";

		public LoadNewSuppressionFileOperation(string filePath, string projectName)
		{
			_filePath = filePath;
			_projectName = projectName.CheckIfNullOrWhiteSpace(nameof(projectName));
		}

		public override void Apply(Workspace workspace, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SuppressionFile suppressionFile = _filePath.IsNullOrWhiteSpace()
				? null
				: SuppressionManager.Instance?.LoadSuppressionFileFrom(_filePath);

			if (suppressionFile == null)
			{
				ShowErrorForFileNotFoundMessage();
			}
		}

		private void ShowErrorForFileNotFoundMessage()
		{
			var errorMessage = new LocalizableResourceString(nameof(Resources.DiagnosticSuppression_FailedToFindSuppressionFile),
																    Resources.ResourceManager, typeof(Resources), _projectName);
			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage.ToString()}");
		}
	}
}
