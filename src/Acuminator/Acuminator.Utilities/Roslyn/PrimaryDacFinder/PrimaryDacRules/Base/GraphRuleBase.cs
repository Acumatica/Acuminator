using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base
{
	/// <summary>
	/// A rule to filter DAC based on a graph.
	/// </summary>
	public abstract class GraphRuleBase : PrimaryDacRuleBase
	{
		/// <summary>
		/// The rule kind.
		/// </summary>
		public sealed override PrimaryDacRuleKind RuleKind => PrimaryDacRuleKind.Graph;

		protected GraphRuleBase(double? customWeight) : base(customWeight)
		{
		}

		/// <summary>
		/// Filter DACs from graph using this rule.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <returns/>
		public abstract IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder);
	}
}