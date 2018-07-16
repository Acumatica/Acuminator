using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Analyzers;


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A default factory to create primary DAC rules.
	/// </summary>
	internal class DefaultRulesProvider : IRulesProvider
	{
		private const int First_1_ViewWeight = 20;
		private const int First_5_ViewWeight = 15;
		private const int First_10_ViewWeight = 10;

		private readonly ImmutableArray<PrimaryDacRuleBase> rules = new List<PrimaryDacRuleBase>
		{
			new PrimaryDacSpecifiedGraphRule(),

			new FirstViewsInGraphRule(numberOfViews: 1, weight: First_1_ViewWeight),
			new FirstViewsInGraphRule(numberOfViews: 5, weight: First_5_ViewWeight),
			new FirstViewsInGraphRule(numberOfViews: 10, weight: First_10_ViewWeight),


		}
		.ToImmutableArray();

		public ImmutableArray<PrimaryDacRuleBase> GetRules() => rules;
	}
}