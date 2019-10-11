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

			var operations = new List<CodeActionOperation>(3);
			SuppressionFile suppressionFile = SuppressionManager.Instance.GetSuppressionFile(project.Name);

			if (suppressionFile == null)
			{
				var operationsToCreateSuppresionFile = GetOperationsToCreateSuppressionFile(project);

				if (operationsToCreateSuppresionFile == null)
					return null;

				operations.AddRange(operationsToCreateSuppresionFile);
			}

			operations.Add(new SuppressInSuppressionFileOperation(Diagnostic, project.Name, semanticModel));
			return operations;
		}

		private IEnumerable<CodeActionOperation> GetOperationsToCreateSuppressionFile(Project project)
		{
			string suppressionFileName = project.Name + SuppressionFile.SuppressionFileExtension;
			TextDocument projectSuppressionFile =
				project.AdditionalDocuments.FirstOrDefault(d => string.Equals(suppressionFileName, d.Name, StringComparison.OrdinalIgnoreCase)) ??
				SuppressionManager.CreateRoslynAdditionalFile(project);

			if (projectSuppressionFile == null)
				return null;

			Solution changedSolution = projectSuppressionFile.Project.Solution;
			return new CodeActionOperation[]
			{
				new ApplyChangesOperation(changedSolution),
				new LoadNewSuppressionFileOperation(projectSuppressionFile.FilePath, project.Name),
			};
		}
	}
}
