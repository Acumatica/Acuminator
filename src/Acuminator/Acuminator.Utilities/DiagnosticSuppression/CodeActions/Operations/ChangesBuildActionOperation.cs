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
	/// A code action operation to change build action of new suppression file.
	/// </summary>
	internal class ChangesBuildActionOperation : CodeActionOperation
	{
		private const string AdditionalFilesBuildAction = "AdditionalFiles";
		private readonly string _filePath;
		private readonly string _buildActionToSet;

		public override string Title => "Change build action for the new suppression file code action operation";

		public ChangesBuildActionOperation(string filePath, string buildActionToSet = null)
		{
			_filePath = filePath;
			_buildActionToSet = buildActionToSet.IsNullOrWhiteSpace()
				? AdditionalFilesBuildAction
				: buildActionToSet;
		}

		public override void Apply(Workspace workspace, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SuppressionFile suppressionFile = _filePath.IsNullOrWhiteSpace()
				? null
				: SuppressionManager.Instance?.LoadSuppressionFileFrom(_filePath);

			if (suppressionFile == null || SuppressionManager.Instance?.BuildActionSetter == null)
				return;

			try
			{
				SuppressionManager.Instance.BuildActionSetter.SetBuildAction(_filePath, _buildActionToSet);
			}
			catch (Exception)
			{
			}
		}
	}
}
