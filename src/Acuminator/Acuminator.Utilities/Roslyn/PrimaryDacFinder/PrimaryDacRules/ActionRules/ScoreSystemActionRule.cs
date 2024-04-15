#nullable enable

using Acuminator.Utilities.Roslyn.PXSystemActions;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ActionRules
{
	/// <summary>
	/// A rule to  add score to DACs which has system action declared for it.
	/// </summary>
	public class ScoreSystemActionRule(PXContext context, double? weight = null) : ScoreSimpleActionRule(weight)
	{
		private readonly PXSystemActionsRegister _systemActionsRegister = new(context);

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol action, INamedTypeSymbol actionType) =>
			base.SatisfyRule(dacFinder, action, actionType) && 
			_systemActionsRegister.IsSystemAction(actionType);
	}
}