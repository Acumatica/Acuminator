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

namespace Acuminator.Utilities.Roslyn.CodeActions
{
	/// <summary>
	/// A solution change simple action with optional preview.
	/// </summary>
	public class SolutionChangeActionWithOptionalPreview : SimpleCodeActionWithOptionalPreview
	{
		private readonly Func<CancellationToken, Task<Solution>> _createChangedSolution;

		public SolutionChangeActionWithOptionalPreview(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, bool displayPreview,
													   string equivalenceKey = null) :
												  base(title, equivalenceKey, displayPreview)
		{
			_createChangedSolution = createChangedSolution.CheckIfNull(nameof(createChangedSolution));
		}

		protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
		{
			return _createChangedSolution(cancellationToken);
		}
	}
}
