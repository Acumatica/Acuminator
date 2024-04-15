#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.RulesProvider;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to add scores to DACs from first N views in graph.
	/// </summary>
	public class FirstViewsInGraphRule : GraphRuleBase
	{
		public override sealed bool IsAbsolute => false;

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

		public override IEnumerable<ITypeSymbol?> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder.GraphViews.Length == 0)
				return [];

			return dacFinder.GraphViews.Take(NumberOfViews)
									   .Select(viewInfo => viewInfo.DAC);
		}
	}
}