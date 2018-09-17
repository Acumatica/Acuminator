using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to select primary DAC when graph has primary DAC specified.
	/// </summary>
	public class PrimaryDacSpecifiedGraphRule : GraphRuleBase
	{
		public override bool IsAbsolute => true;

		public PrimaryDacSpecifiedGraphRule() : base(null)
		{
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder?.Graph == null)
				return Enumerable.Empty<ITypeSymbol>();

			ITypeSymbol primaryDac = dacFinder.Graph.GetDeclaredPrimaryDacFromGraphOrGraphExtension(dacFinder.PxContext);
			return primaryDac?.ToEnumerable() ?? Enumerable.Empty<ITypeSymbol>();
		}
	}
}