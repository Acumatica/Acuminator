using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Analyzers;


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A rule to add scores to DACs from first N views in graph.
	/// </summary>
	public class FirstViewsInGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => false;

		/// <summary>
		/// The number of first views to select from graph.
		/// </summary>
		public int NumberOfViews { get; }

		public override double Weight { get; }

		public FirstViewsInGraphRule(int numberOfViews, int weight)
		{
			if (weight == 0)
				throw new ArgumentOutOfRangeException(nameof(weight), "Rule weight can't be zero");
			else if (numberOfViews <= 0)
				throw new ArgumentOutOfRangeException(nameof(numberOfViews), "Number of views should be positive");

			Weight = weight;
			NumberOfViews = numberOfViews;
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder, INamedTypeSymbol graph)
		{
			if (graph == null || dacFinder == null || dacFinder.GraphViewSymbolsWithTypes.Length == 0 ||
				dacFinder.CancellationToken.IsCancellationRequested)
			{
				return Enumerable.Empty<ITypeSymbol>();
			}

			return dacFinder.GraphViewSymbolsWithTypes.Take(NumberOfViews)
													  .Select(viewWithType => viewWithType.ViewType.GetDacFromView(dacFinder.PxContext))
													  .Where(dac => dac != null);
		}
	}
}