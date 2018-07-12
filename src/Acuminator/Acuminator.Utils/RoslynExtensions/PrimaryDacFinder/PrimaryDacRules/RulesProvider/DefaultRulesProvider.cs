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
		private readonly ImmutableArray<PrimaryDacRuleBase> rules = new List<PrimaryDacRuleBase>
		{
			new PrimaryDacSpecifiedGraphRule()
		}
		.ToImmutableArray();

		public ImmutableArray<PrimaryDacRuleBase> GetRules() => rules;
	}
}