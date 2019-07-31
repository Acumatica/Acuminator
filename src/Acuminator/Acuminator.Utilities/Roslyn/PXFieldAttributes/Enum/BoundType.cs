using System;


namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Enumeration used to classify Acumatica attributes.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public enum BoundType
	{
		/// <summary>
		/// The attribute is not related to the database binding.
		/// </summary>	
		NotDefined = 0,

		/// <summary>
		/// The attribute is database unbound.
		/// </summary>
		Unbound = 1,

		/// <summary>
		/// The attribute is database bound.
		/// </summary>
		DbBound = 2,

		/// <summary>
		/// The attribute classification is unknown and cannot be deduced by means of static analysis.
		/// </summary>
		Unknown = 4
	}


	public static class BoundTypeCombiner
	{
		public static BoundType Combine(this BoundType x, BoundType y) => x >= y ? x : y;
	}
}