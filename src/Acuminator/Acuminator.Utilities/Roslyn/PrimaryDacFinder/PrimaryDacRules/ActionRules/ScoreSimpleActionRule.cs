using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ActionRules
{
	/// <summary>
	/// A rule to  add score to DACs which has action declared for it.
	/// </summary>
	public class ScoreSimpleActionRule : ActionRuleBase
	{
		public sealed override bool IsAbsolute => false;

		public ScoreSimpleActionRule(double? weight = null) : base(weight)
		{
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol action, INamedTypeSymbol actionType)
		{
			if (dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested || actionType == null)
				return false;

			return true;
		}
	}
}