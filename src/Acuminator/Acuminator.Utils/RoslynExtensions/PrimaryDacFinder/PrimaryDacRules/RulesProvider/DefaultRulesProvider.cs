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
		

		private readonly ImmutableArray<PrimaryDacRuleBase> rules;

		public DefaultRulesProvider(PXContext context)
		{
			context.ThrowOnNull(nameof(context));

			rules = new List<PrimaryDacRuleBase>
			{
				//AbsoluteRules
				new PrimaryDacSpecifiedGraphRule(),
				new PXImportAttributeGraphRule(),

				// Heuristic rules
				// Graph rules
				new FirstViewsInGraphRule(numberOfViews: 1),
				new FirstViewsInGraphRule(numberOfViews: 3),
				new FirstViewsInGraphRule(numberOfViews: 5),
				new FirstViewsInGraphRule(numberOfViews: 10),
				new NoReadOnlyViewGraphRule(),
				new ViewsWithoutPXViewNameAttributeGraphRule(context),

				// View rules
				new ForbiddenWordsInNameViewRule(),
				new HiddenAttributesViewRule(),
				new NoPXSetupViewRule(context),
				new PXViewNameAttributeViewRule(context),			

				// Action rules
				new ScoreSimpleActionRule(),
				new ScoreSystemActionRule(context),

				// DAC rules
				new SameOrDescendingNamespaceDacRule()
			}
			.ToImmutableArray();
		}

		public ImmutableArray<PrimaryDacRuleBase> GetRules() => rules;
	}
}