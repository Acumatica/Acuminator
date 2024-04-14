#nullable enable

using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base
{
	/// <summary>
	/// A rule to filter DAC based on a graph's view.
	/// </summary>
	public abstract class ViewRuleBase : PrimaryDacRuleBase
	{
		/// <summary>
		/// The rule kind.
		/// </summary>
		public override sealed PrimaryDacRuleKind RuleKind => PrimaryDacRuleKind.View;

		protected ViewRuleBase(double? customWeight) : base(customWeight)
		{
		}

		/// <summary>
		/// Query if view satisfies this rule's conditions.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="viewInfo">The view info.</param>
		/// <returns/>
		public abstract bool SatisfyRule(PrimaryDacFinder dacFinder, DataViewInfo viewInfo);
	}
}