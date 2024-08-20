﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.DiagnosticSuppression.CodeActions
{
	/// <summary>
	/// A code action operation to suppress diagnostic in the suppression file.
	/// </summary>
	internal class SuppressInSuppressionFileOperation : SuppressionOperationBase
	{	
		private readonly Diagnostic _diagnosticToSuppress;
		private readonly SemanticModel _semanticModel;

		public override string Title => "Suppress diagnostic code action in the suppression file operation";

		public SuppressInSuppressionFileOperation(Diagnostic diagnosticToSuppress, string assemblyName, SemanticModel semanticModel) :
											 base(assemblyName)
		{		
			_diagnosticToSuppress = diagnosticToSuppress.CheckIfNull();
			_semanticModel = semanticModel.CheckIfNull();
		}

		public override void Apply(Workspace workspace, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SuppressionFile? suppressionFile = SuppressionManager.Instance?.GetSuppressionFile(AssemblyName);

			if (suppressionFile == null)
			{
				ShowLocalizedError(nameof(Resources.DiagnosticSuppression_FailedToFindSuppressionFile), AssemblyName);
				return;
			}

			cancellationToken.ThrowIfCancellationRequested();

			if (!SuppressionManager.SuppressDiagnostic(_semanticModel, _diagnosticToSuppress.Id, _diagnosticToSuppress.Location.SourceSpan,
														_diagnosticToSuppress.DefaultSeverity, cancellationToken))
			{
				ShowLocalizedError(nameof(Resources.DiagnosticSuppression_FailedToAddToSuppressionFile), suppressionFile.Path);
			}
		}
	}
}
