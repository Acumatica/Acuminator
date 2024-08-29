#nullable enable

using System.Linq;

using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.DacRules
{
	/// <summary>
	/// A rule to add score to DACs which are declared in the same namespace as the graph for which the primary DAC is calculated.
	/// </summary>
	public class SameOrDescendingNamespaceDacRule(double? weight = null) : DacRuleBase(weight)
	{
		public sealed override bool IsAbsolute => false;

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ITypeSymbol dac)
		{
			dacFinder.CancellationToken.ThrowIfCancellationRequested();

			if (dacFinder.GraphSemanticModel.GraphSymbol?.ContainingNamespace == null)
				return false;

			var graphNamespace = dacFinder.GraphSemanticModel.GraphSymbol.ContainingNamespace;
			return dac.GetContainingNamespaces().Contains(graphNamespace, SymbolEqualityComparer.Default);
		}
	}
}