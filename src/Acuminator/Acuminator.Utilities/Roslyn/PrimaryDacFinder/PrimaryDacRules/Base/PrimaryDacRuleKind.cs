namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base
{
	/// <summary>
	/// Values that represent kinds of rule of the graph's primary DAC.
	/// </summary>
	public enum PrimaryDacRuleKind
	{
		/// <summary>
		/// The rule is related to the graph.
		/// </summary>
		Graph,

		/// <summary>
		/// The rule is related to the graph's view.
		/// </summary>
		View,

		/// <summary>
		/// The rule is related to the graph's DAC.
		/// </summary>
		Dac,

		/// <summary>
		/// The rule is related to the graph's action.
		/// </summary>
		Action
	}
}
