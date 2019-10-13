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
	internal class ChangesBuildActionOperation : SuppressionOperationBase
	{
		private const string AdditionalFilesBuildAction = "AdditionalFiles";
		private readonly string _buildActionToSet;

		public override string Title => "Change build action for the new suppression file code action operation";

		public ChangesBuildActionOperation(string projectName, string buildActionToSet = null) : base(projectName)
		{
			_buildActionToSet = buildActionToSet.IsNullOrWhiteSpace()
				? AdditionalFilesBuildAction
				: buildActionToSet;
		}

		public override void Apply(Workspace workspace, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (SuppressionManager.Instance?.BuildActionSetter == null)
				return;

			SuppressionFile suppressionFile = SuppressionManager.Instance?.GetSuppressionFile(ProjectName);

			if (suppressionFile == null)
			{
				ShowLocalizedError(nameof(Resources.DiagnosticSuppression_FailedToFindSuppressionFileToSetBuildAction), ProjectName);
				return;
			}

			bool successfullySetBuldAction;

			try
			{
				successfullySetBuldAction = 
					SuppressionManager.Instance.BuildActionSetter.SetBuildAction(suppressionFile.Path, _buildActionToSet);
			}
			catch (Exception)
			{
				successfullySetBuldAction = false;
			}

			if (!successfullySetBuldAction)
			{
				ShowLocalizedError(nameof(Resources.DiagnosticSuppression_FailedToSetBuildAction), suppressionFile.Path);
			}
		}
	}
}