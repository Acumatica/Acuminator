using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.RulesProvider;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
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

		protected override double DefaultWeight => 0.0;

		public FirstViewsInGraphRule(int numberOfViews, double? weight = null) : base(weight)
		{
			if (weight == 0)
				throw new ArgumentOutOfRangeException(nameof(weight), "Rule weight can't be zero");
			else if (numberOfViews <= 0)
				throw new ArgumentOutOfRangeException(nameof(numberOfViews), "Number of views should be positive");

			NumberOfViews = numberOfViews;
			
			if (weight == null)
			{
				Weight = WeightsTable.Default[$"{nameof(FirstViewsInGraphRule)}-{NumberOfViews}"];
			}
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder == null || dacFinder.GraphViewSymbolsWithTypes.Length == 0 ||
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