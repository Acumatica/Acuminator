using System;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.DiagnosticSuppression.CodeActions
{
	/// <summary>
	/// A code action operation to load new suppression file by suppression manager after it was added to workspace.
	/// </summary>
	internal class LoadNewSuppressionFileOperation : SuppressionOperationBase
	{
		private readonly string _filePath;

		public override string Title => "Load new suppression file code action operation";

		public LoadNewSuppressionFileOperation(string filePath, string projectName) : base(projectName)
		{
			_filePath = filePath;
		}

		public override void Apply(Workspace workspace, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SuppressionFile suppressionFile = _filePath.IsNullOrWhiteSpace()
				? null
				: SuppressionManager.Instance?.LoadSuppressionFileFrom(_filePath);

			if (suppressionFile == null)
			{
				ShowLocalizedError(nameof(Resources.DiagnosticSuppression_FailedToFindSuppressionFile), ProjectName);
			}
		}				
	}
}
