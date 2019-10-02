using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Utilities.DiagnosticSuppression.CodeActions
{
	public class NoPreviewCodeAction : CodeAction
	{
		private readonly Func<CancellationToken, Task<Solution>> createChangedSolution;

		public override string Title { get; }

		public override string EquivalenceKey { get; }

		public NoPreviewCodeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution,
								   string equivalenceKey = null)
		{
			this.createChangedSolution = createChangedSolution;

			Title = title;
			EquivalenceKey = equivalenceKey;
		}

		protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(
			CancellationToken cancellationToken)
		{
			return Task.FromResult(Enumerable.Empty<CodeActionOperation>());
		}

		protected override Task<Solution> GetChangedSolutionAsync(
			CancellationToken cancellationToken)
		{
			return createChangedSolution(cancellationToken);
		}
	}
}
