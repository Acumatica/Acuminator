using System;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.CodeActions
{
	/// <summary>
	/// A solution change simple action with optional preview.
	/// </summary>
	public class SolutionChangeActionWithOptionalPreview : SimpleCodeActionWithOptionalPreview
	{
		private readonly Func<CancellationToken, Task<Solution?>> _createChangedSolution;

		public SolutionChangeActionWithOptionalPreview(string title, Func<CancellationToken, Task<Solution?>> createChangedSolution, bool displayPreview,
													   string? equivalenceKey = null) :
												  base(title, equivalenceKey, displayPreview)
		{
			_createChangedSolution = createChangedSolution.CheckIfNull();
		}

		protected override Task<Solution?> GetChangedSolutionAsync(CancellationToken cancellationToken) => 
			_createChangedSolution(cancellationToken);
	}
}
