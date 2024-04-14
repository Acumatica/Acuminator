#nullable enable

using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to select primary DAC when graph has primary DAC specified.
	/// </summary>
	public class PrimaryDacSpecifiedGraphRule() : GraphRuleBase(customWeight: null)
	{
		public override bool IsAbsolute => true;

		public override IEnumerable<ITypeSymbol?> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder.GraphSemanticModel.GraphSymbol == null)
				return [];

			ITypeSymbol? primaryDac = dacFinder.GraphSemanticModel.GraphSymbol
																  .GetDeclaredPrimaryDacFromGraphOrGraphExtension(dacFinder.PxContext);
			return primaryDac != null ? [primaryDac] : [];
		}
	}
}