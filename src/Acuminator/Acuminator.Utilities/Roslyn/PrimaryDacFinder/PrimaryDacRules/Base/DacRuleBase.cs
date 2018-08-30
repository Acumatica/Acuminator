using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base
{
	/// <summary>
	/// A rule to select DAC based on rule's conditions.
	/// </summary>
	public abstract class DacRuleBase : PrimaryDacRuleBase
	{
		/// <summary>
		/// The rule kind.
		/// </summary>
		public sealed override PrimaryDacRuleKind RuleKind => PrimaryDacRuleKind.Dac;

		protected DacRuleBase(double? customWeight) : base(customWeight)
		{
		}

		/// <summary>
		/// Query if DAC satisfies this rule's conditions.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="dac">The DAC.</param>
		/// <returns/>
		public abstract bool SatisfyRule(PrimaryDacFinder dacFinder, ITypeSymbol dac);
	}
}