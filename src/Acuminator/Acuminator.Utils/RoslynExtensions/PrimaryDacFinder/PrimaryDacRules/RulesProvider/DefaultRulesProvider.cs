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
		private const double First_1_ViewsWeight = 1;
		private const double First_3_ViewsWeight = 5;
		private const double First_5_ViewsWeight = 5;
		private const double First_10_ViewsWeight = 10;

		private readonly ImmutableArray<PrimaryDacRuleBase> rules;

		public DefaultRulesProvider(PXContext context)
		{
			context.ThrowOnNull(nameof(context));

			rules = new List<PrimaryDacRuleBase>
			{
				new PrimaryDacSpecifiedGraphRule(),
				new PXImportAttributeGraphRule(),

				new FirstViewsInGraphRule(numberOfViews: 1, weight: First_1_ViewsWeight),
				new FirstViewsInGraphRule(numberOfViews: 3, weight: First_3_ViewsWeight),
				new FirstViewsInGraphRule(numberOfViews: 5, weight: First_5_ViewsWeight),
				new FirstViewsInGraphRule(numberOfViews: 10, weight: First_10_ViewsWeight),
				new NoReadOnlyViewGraphRule(),

				new ForbiddenWordsInNameViewRule(),
				new HiddenAttributesViewRule(),
				new NoPXSetupViewRule(context),

				new ScoreSimpleActionRule()
			}
			.ToImmutableArray();
		}

		public ImmutableArray<PrimaryDacRuleBase> GetRules() => rules;
	}
}