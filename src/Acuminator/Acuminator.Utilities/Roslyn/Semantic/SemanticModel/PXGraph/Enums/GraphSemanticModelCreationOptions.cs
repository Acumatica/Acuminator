using System;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Values that represent graph semantic model creation options.
	/// </summary>
	[Flags]
	public enum GraphSemanticModelCreationOptions
	{
		/// <summary>
		/// A binary constant representing the collect general graph Information flag.
		/// </summary>
		CollectGeneralGraphInfo = 0,
	
		/// <summary>
		/// A binary constant representing the collect processing delegates flag.
		/// </summary>
		CollectProcessingDelegates = 0b_00001,
		
		/// <summary>
		/// A binary constant representing the infer implicit models flag.
		/// </summary>
		InferImplicitModels = 0b_00010,

		/// <summary>
		/// Collect all possible semantic data during construction of the graph semantic model.
		/// </summary>
		CollectAll = CollectGeneralGraphInfo | CollectProcessingDelegates | InferImplicitModels
	}


	public static class GraphSemanticModelCreationOptionsUtils
	{
		public static bool HasOption(this GraphSemanticModelCreationOptions creationOptions, GraphSemanticModelCreationOptions flagToCheck) =>
			(creationOptions & flagToCheck) == flagToCheck;
	}
}
