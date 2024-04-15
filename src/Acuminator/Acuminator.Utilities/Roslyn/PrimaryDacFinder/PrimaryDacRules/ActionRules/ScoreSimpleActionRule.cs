#nullable enable
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ActionRules
{
	/// <summary>
	/// A rule to  add score to DACs which has action declared for it.
	/// </summary>
	public class ScoreSimpleActionRule(double? weight = null) : ActionRuleBase(weight)
	{
		public override sealed bool IsAbsolute => false;

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol action, INamedTypeSymbol actionType)
		{
			dacFinder.CancellationToken.ThrowIfCancellationRequested();
			return true;
		}
	}
}