using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base
{
	/// <summary>
	/// A rule to filter DAC based on a graph's action.
	/// </summary>
	public abstract class ActionRuleBase : PrimaryDacRuleBase
	{
		/// <summary>
		/// The rule kind.
		/// </summary>
		public sealed override PrimaryDacRuleKind RuleKind => PrimaryDacRuleKind.Action;

		protected ActionRuleBase(double? customWeight) : base(customWeight)
		{
		}

		/// <summary>
		/// Query if action satisfies this rule's conditions.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="action">The action.</param>
		/// <param name="actionType">Type of the action.</param>
		/// <returns/>
		public abstract bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol action, INamedTypeSymbol actionType);
	}
}