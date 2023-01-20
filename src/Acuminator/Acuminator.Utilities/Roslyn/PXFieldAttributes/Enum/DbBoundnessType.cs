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
		/// The attribute application is PXDBScalar and is a formula calced on DB Side.
		/// </summary>
		PXDBScalar = 4,

		/// <summary>
		/// The attribute application is PXDBCalced and is a sub-query calced on DB Side.
		/// </summary>
		PXDBCalced = 8,

		/// <summary>
		/// The attribute application classification is unknown and cannot be deduced by means of static analysis.
		/// </summary>
		Unknown = 16
	}


	public static class DbBoundnessTypeCombiner
	{
		public static DbBoundnessType Combine(this DbBoundnessType x, DbBoundnessType y)
		{
			if (x == y)
				return x;
			else if (y == DbBoundnessType.Unknown)
				return y;

			switch (x) 
			{
				case DbBoundnessType.Unknown:
					return x;

				case DbBoundnessType.NotDefined:
				case DbBoundnessType.Unbound:
					return x >= y ? x : y;

				case DbBoundnessType.DbBound:
					return x >= y ? x : DbBoundnessType.Unknown;

				case DbBoundnessType.PXDBScalar:
					switch (y)
					{
						case DbBoundnessType.DbBound:
						case DbBoundnessType.PXDBCalced:
						case DbBoundnessType.Unknown:
							return DbBoundnessType.Unknown;
						default:
							return x;
					}

				case DbBoundnessType.PXDBCalced:
					switch (y)
					{
						case DbBoundnessType.DbBound:
						case DbBoundnessType.PXDBScalar:
						case DbBoundnessType.Unknown:
							return DbBoundnessType.Unknown;
						default:
							return x;
					}

				default:
					return DbBoundnessType.Unknown;
			}
		}			
	}
}