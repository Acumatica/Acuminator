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
		Unknown = 16,
		
		/// <summary>
		/// The attribute application has self contradictory DB boundness proved by the code analysis.
		/// </summary>
		/// <remarks>
		/// This flag is used to mark attributes and DAC properties with wrong boundness
		/// </remarks>
		Error = 32
	}


	public static class DbBoundnessTypeCombiner
	{
		public static DbBoundnessType Combine(this DbBoundnessType x, DbBoundnessType y)
		{
			// minor optimizations for frequent cases
			if (x == y || y == DbBoundnessType.NotDefined)
				return x;
			
			switch (x) 
			{
				case DbBoundnessType.NotDefined:
					return y;

				case DbBoundnessType.Unbound:
					return y.CombineWithUnbound();

				case DbBoundnessType.DbBound:
					return y.CombineWithDbBound();

				case DbBoundnessType.PXDBScalar:
					return y.CombineWithPXDBScalar();

				case DbBoundnessType.PXDBCalced:
					return y.CombineWithPXDBCalced();

				case DbBoundnessType.Error:
					return x;

				case DbBoundnessType.Unknown:
				default:
					return y == DbBoundnessType.Error
						? DbBoundnessType.Error
						: DbBoundnessType.Unknown;
			}
		}

		private static DbBoundnessType CombineWithUnbound(this DbBoundnessType y)
		{
			switch (y)
			{
				case DbBoundnessType.NotDefined:
				case DbBoundnessType.Unbound:
					return DbBoundnessType.Unbound;

				case DbBoundnessType.DbBound:
					return DbBoundnessType.DbBound;

				case DbBoundnessType.PXDBCalced:
					return DbBoundnessType.PXDBCalced;

				case DbBoundnessType.PXDBScalar:
				case DbBoundnessType.Error:
					return DbBoundnessType.Error;

				case DbBoundnessType.Unknown:
				default:
					return DbBoundnessType.Unknown;
			}
		}

		private static DbBoundnessType CombineWithDbBound(this DbBoundnessType y)
		{
			if (y <= DbBoundnessType.DbBound)
				return DbBoundnessType.DbBound;
			else if (y == DbBoundnessType.Unknown)
				return DbBoundnessType.Unknown;
			else
				return DbBoundnessType.Error;
		}

		private static DbBoundnessType CombineWithPXDBScalar(this DbBoundnessType y)
		{
			switch (y)
			{
				case DbBoundnessType.Unbound:
				case DbBoundnessType.DbBound:
				case DbBoundnessType.PXDBCalced:
				case DbBoundnessType.Error:
					return DbBoundnessType.Error;

				case DbBoundnessType.Unknown:
					return DbBoundnessType.Unknown;

				default:
					return DbBoundnessType.PXDBScalar;
			}
		}

		private static DbBoundnessType CombineWithPXDBCalced(this DbBoundnessType y)
		{
			switch (y)
			{
				case DbBoundnessType.DbBound:
				case DbBoundnessType.PXDBScalar:
				case DbBoundnessType.Error:
					return DbBoundnessType.Error;

				case DbBoundnessType.Unknown:
					return DbBoundnessType.Unknown;

				default:
					return DbBoundnessType.PXDBCalced;
			}
		}
	}
}