using System.Linq;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.DacRules
{
	/// <summary>
	/// A rule to  add score to DACs which has action declared for it.
	/// </summary>
	public class SameOrDescendingNamespaceDacRule : DacRuleBase
	{
		public sealed override bool IsAbsolute => false;

		public SameOrDescendingNamespaceDacRule(double? weight = null) : base(weight)
		{
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ITypeSymbol dac)
		{
			if (dacFinder?.Graph?.ContainingNamespace == null || dacFinder.CancellationToken.IsCancellationRequested ||
				dac?.ContainingNamespace == null)
			{
				return false;
			}

			var graphNameSpace = dacFinder.Graph.ContainingNamespace;
			return dac.GetContainingNamespaces().Contains(graphNameSpace);
		}
	}
}