using System;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Values that represent graph semantic model creation options.
	/// </summary>
	public enum GraphSemanticModelCreationOptions
	{
		/// <summary>
		/// Collect all possible semantic data during construction of the graph semantic model.
		/// </summary>
		CollectAll,

		/// <summary>
		/// Do not collect graph init delegates.
		/// </summary>
		DoNotCollectInitDelegates
	}
}
