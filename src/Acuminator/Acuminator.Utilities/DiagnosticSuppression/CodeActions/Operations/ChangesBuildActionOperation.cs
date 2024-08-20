﻿using System;
using System.Threading;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

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

		public ChangesBuildActionOperation(string assemblyName, string? buildActionToSet = null) : base(assemblyName)
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

			SuppressionFile? suppressionFile = SuppressionManager.Instance?.GetSuppressionFile(AssemblyName);

			if (suppressionFile == null)
			{
				ShowLocalizedError(nameof(Resources.DiagnosticSuppression_FailedToFindSuppressionFileToSetBuildAction), AssemblyName);
				return;
			}

			bool successfullySetBuldAction;

			try
			{
				successfullySetBuldAction = 
					SuppressionManager.Instance?.BuildActionSetter.SetBuildAction(suppressionFile.Path, _buildActionToSet) ?? false;
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