using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace Acuminator.Utilities.PrimaryDAC
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