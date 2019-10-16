using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Acuminator.Utilities.DiagnosticSuppression.CodeActions
{
	/// <summary>
	/// A "Suppress with suppression file" code action.
	/// </summary>
	[System.Runtime.InteropServices.Guid("D9366D8E-C09F-40AD-83E7-D9DE323A15F4")]
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

			SuppressionFile suppressionFile = SuppressionManager.Instance.GetSuppressionFile(project.AssemblyName);

			if (suppressionFile == null)
			{
				IEnumerable<CodeActionOperation> operationsToCreateSuppresionFile = GetOperationsToCreateSuppressionFile(project);

				if (operationsToCreateSuppresionFile == null)
					return null;

				var operationsWithSuppressionFileCreation = new List<CodeActionOperation>(4);
				operationsWithSuppressionFileCreation.AddRange(operationsToCreateSuppresionFile);
				operationsWithSuppressionFileCreation.Add(new SuppressInSuppressionFileOperation(Diagnostic, project.AssemblyName, semanticModel));
				return operationsWithSuppressionFileCreation;
			}
			else
			{
				// For some reason the changes in suppression file will immediately reflect in the code editor 
				// only if we call suppress diagnostic in code action manually, not via code action operation
				if (!SuppressionManager.SuppressDiagnostic(semanticModel, Diagnostic.Id, Diagnostic.Location.SourceSpan,
														   Diagnostic.DefaultSeverity, cancellationToken))
				{
					var errorMessage = new LocalizableResourceString(nameof(Resources.DiagnosticSuppression_FailedToAddToSuppressionFile),
																	 Resources.ResourceManager, typeof(Resources), suppressionFile.Path);
					System.Diagnostics.Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage}");
				}

				return new List<CodeActionOperation>(1)
				{
					new ApplyChangesOperation(project.Solution)
				};
			}
		}

		private IEnumerable<CodeActionOperation> GetOperationsToCreateSuppressionFile(Project project)
		{
			string suppressionFileName = project.AssemblyName + SuppressionFile.SuppressionFileExtension;
			TextDocument projectSuppressionFile =
				project.AdditionalDocuments.FirstOrDefault(d => string.Equals(suppressionFileName, d.Name, StringComparison.OrdinalIgnoreCase)) ??
				SuppressionManager.CreateRoslynAdditionalFile(project);

			if (projectSuppressionFile == null)
				return null;

			Solution changedSolution = projectSuppressionFile.Project.Solution;

			if (SuppressionManager.Instance?.BuildActionSetter != null)
			{
				return new CodeActionOperation[]
				{
					new ApplyChangesOperation(changedSolution),
					new LoadNewSuppressionFileOperation(projectSuppressionFile.FilePath, project.AssemblyName),
					new ChangesBuildActionOperation(project.AssemblyName)
				};
			}
			else
			{
				return new CodeActionOperation[]
				{
					new ApplyChangesOperation(changedSolution),
					new LoadNewSuppressionFileOperation(projectSuppressionFile.FilePath, project.AssemblyName)
				};
			}		
		}
	}
}
