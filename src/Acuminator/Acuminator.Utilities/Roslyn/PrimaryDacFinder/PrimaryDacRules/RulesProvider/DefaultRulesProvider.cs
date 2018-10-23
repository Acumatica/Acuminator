using System.Collections.Generic;
using System.Collections.Immutable;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ActionRules;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.DacRules;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.RulesProvider
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
				new PXFilteredProcessingGraphRule(context),

				// Heuristic rules
				// Graph rules
				new FirstViewsInGraphRule(numberOfViews: 1),
				new FirstViewsInGraphRule(numberOfViews: 3),
				new FirstViewsInGraphRule(numberOfViews: 5),
				new FirstViewsInGraphRule(numberOfViews: 10),

				new PairOfViewsWithSpecialNamesGraphRule(firstName: "Document", secondName: "CurrentDocument"),
				new PairOfViewsWithSpecialNamesGraphRule(firstName: "Entities", secondName: "CurrentEntity"),

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