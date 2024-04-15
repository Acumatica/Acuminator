#nullable enable

using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// A rule to filter out views which are PXSetup views.
	/// </summary>
	public class NoPXSetupViewRule(double? weight = null) : ViewRuleBase(weight)
	{
		public override sealed bool IsAbsolute => false;

		/// <summary>
		/// Query if view type is PXSetup view. 
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="view">The view.</param>
		/// <param name="viewType">Type of the view.</param>
		/// <returns/>
		public override sealed bool SatisfyRule(PrimaryDacFinder dacFinder, DataViewInfo viewInfo) =>
			viewInfo.IsSetup;
	}
}