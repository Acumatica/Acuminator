using System.Collections.Immutable;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.RulesProvider
{
	/// <summary>
	/// An interface for factory used to create primary DAC rules.
	/// </summary>
	public interface IRulesProvider 
	{
		/// <summary>
		/// Gets the rules to determine primary DAC.
		/// </summary>
		/// <returns/>
		ImmutableArray<PrimaryDacRuleBase> GetRules();
	}
}