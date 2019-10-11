using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Utilities.DiagnosticSuppression.CodeActions
{
	/// <summary>
	/// A "Suppress with suppression file" code action.
	/// </summary>
	public class SuppressWithSuppressionFileCodeAction : SimpleCodeActionWithOptionalPreview
	{
		protected CodeFixContext Context { get; }

		protected Diagnostic Diagnostic { get; }

		public SuppressWithSuppressionFileCodeAction(CodeFixContext context, Diagnostic diagnostic, 
													 string title, string equivalenceKey = null) :
												base(title, equivalenceKey, displayPreview: false)
		{
			Context = context;

			Diagnostic = diagnostic.CheckIfNull(nameof(diagnostic));
		}

		protected override async Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Project project = Context.Document?.Project;

			if (project == null)
				return null;

			SemanticModel semanticModel = await Context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel == null)
				return null;

			
			SuppressionFile suppressionFile = SuppressionManager.Instance.GetSuppressionFile(project.Name);

			if (suppressionFile == null)
			{
				var operationsToCreateSuppresionFile = GetOperationsToCreateSuppressionFile(project);

				if (operationsToCreateSuppresionFile == null)
					return null;

				operations.AddRange(operationsToCreateSuppresionFile);
			}

			var operations = new List<CodeActionOperation>(2);



			return operations;
		}

		private IEnumerable<CodeActionOperation> GetOperationsToCreateSuppressionFileAndSuppressDiagnostic(Project project)
		{
			string suppressionFileName = project.Name + SuppressionFile.SuppressionFileExtension;
			TextDocument projectSuppressionFile =
				project.AdditionalDocuments.FirstOrDefault(d => string.Equals(suppressionFileName, d.Name, StringComparison.OrdinalIgnoreCase)) ??
				SuppressionManager.CreateRoslynAdditionalFile(project);

			if (projectSuppressionFile == null)
				return null;

			Solution changedSolution = projectSuppressionFile.Project.Solution;
			var operationsToCreateSuppresionFile = new List<CodeActionOperation>(2);
			operationsToCreateSuppresionFile.Add(new ApplyChangesOperation(changedSolution));

			return operationsToCreateSuppresionFile;
		}





		private async Task<Solution> SuppressInSuppressionFileAsync(, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Project project = context.Document.Project;
			

			

			
			

			

			cancellationToken.ThrowIfCancellationRequested();

			if (!suppressionFileExists ||
				!SuppressionManager.SuppressDiagnostic(semanticModel, diagnostic.Id, diagnostic.Location.SourceSpan,
													   diagnostic.DefaultSeverity, cancellationToken))
			{
				ShowErrorMessage(projectSuppressionFile, project);
				return null;
			}

			return projectSuppressionFile.Project.Solution;
		}

		private void ShowErrorMessage(TextDocument suppressionFile, Project project)
		{
			LocalizableResourceString errorMessage;

			if (suppressionFile?.FilePath != null)
			{
				errorMessage = new LocalizableResourceString(nameof(UtilityResources.DiagnosticSuppression_FailedToAddToSuppressionFile),
															 UtilityResources.ResourceManager, typeof(UtilityResources), suppressionFile.FilePath);
			}
			else
			{
				errorMessage = new LocalizableResourceString(nameof(UtilityResources.DiagnosticSuppression_FailedToFindSuppressionFile),
															 UtilityResources.ResourceManager, typeof(UtilityResources), project.Name);
			}

			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage.ToString()}");
		}
	}
}
