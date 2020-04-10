namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Values that represent graph initializer types.
	/// </summary>
	public enum GraphInitializerType
	{
		/// <summary>
		/// An enum constant representing the instance constructor option.
		/// </summary>
		InstanceConstructor,

		/// <summary>
		/// An enum constant representing the initialize method option.
		/// </summary>
		InitializeMethod,

		/// <summary>
		/// An enum constant representing the instance created delegate option.
		/// </summary>
		InstanceCreatedDelegate
	}
}
