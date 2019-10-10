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

namespace Acuminator.Analyzers.CodeActions
{
	/// <summary>
	/// A "Suppress with suppression file" code action.
	/// </summary>
	public class SuppressWithSuppressionFileCodeAction : SimpleCodeActionWithOptionalPreview
	{
		public SuppressWithSuppressionFileCodeAction(string title, string equivalenceKey = null) :
												base(title, equivalenceKey, displayPreview: false)
		{
			
		}

		protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
		{

			return _createChangedSolution(cancellationToken);
		}
	}
}
