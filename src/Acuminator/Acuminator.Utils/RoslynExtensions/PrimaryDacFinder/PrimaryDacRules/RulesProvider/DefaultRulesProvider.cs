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
		private const int First_2_ViewsWeight = 25;
		private const int First_5_ViewsWeight = 20;
		private const int First_10_ViewsWeight = 15;

		private readonly ImmutableArray<PrimaryDacRuleBase> rules;

		public DefaultRulesProvider(PXContext context)
		{
			context.ThrowOnNull(nameof(context));

			rules = new List<PrimaryDacRuleBase>
			{
				new PrimaryDacSpecifiedGraphRule(),

				new FirstViewsInGraphRule(numberOfViews: 2, weight: First_2_ViewsWeight),
				new FirstViewsInGraphRule(numberOfViews: 5, weight: First_5_ViewsWeight),
				new FirstViewsInGraphRule(numberOfViews: 10, weight: First_10_ViewsWeight),
				new NoReadOnlyViewGraphRule(),

				new ForbiddenWordsInNameViewRule(),
				new HiddenAttributesViewRule(),
				new NoPXSetupViewRule(context)
			}
			.ToImmutableArray();
		}

		public ImmutableArray<PrimaryDacRuleBase> GetRules() => rules;
	}
}