using Acuminator.Utilities.Roslyn.PXSystemActions;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ActionRules
{
	/// <summary>
	/// A rule to  add score to DACs which has system action declared for it.
	/// </summary>
	public class ScoreSystemActionRule : ScoreSimpleActionRule
	{
		private readonly PXSystemActionsRegister systemActionsRegister;

		public ScoreSystemActionRule(PXContext context, double? weight = null) : base(weight)
		{
			systemActionsRegister = new PXSystemActionsRegister(context);
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol action, INamedTypeSymbol actionType)
		{
			if (!base.SatisfyRule(dacFinder, action, actionType))
				return false;

			return systemActionsRegister.IsSystemAction(actionType);
		}
	}
}