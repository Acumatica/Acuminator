using System;


namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Enumeration used to classify Acumatica attributes applied to a DAC field property by their DB boundness.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public enum DbBoundnessType
	{
		/// <summary>
		/// The attribute application is not related to the database binding.
		/// </summary>	
		NotDefined = 0,

		/// <summary>
		/// The attribute application is database unbound.
		/// </summary>
		Unbound = 1,

		/// <summary>
		/// The attribute application is database bound.
		/// </summary>
		DbBound = 2,

		/// <summary>
		/// The attribute application classification is unknown and cannot be deduced by means of static analysis.
		/// </summary>
		Unknown = 4
	}


	public static class DbBoundnessTypeCombiner
	{
		public static DbBoundnessType Combine(this DbBoundnessType x, DbBoundnessType y) => x >= y ? x : y;
	}
}