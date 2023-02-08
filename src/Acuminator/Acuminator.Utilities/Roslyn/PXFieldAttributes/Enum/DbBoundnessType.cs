#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

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
		/// <summary>
		/// Combines a collection of <see cref="DbBoundnessType"/> in a proper order.
		/// </summary>
		/// <remarks>
		/// The <see cref="DbBoundnessTypeCombiner.Combine(DbBoundnessType, DbBoundnessType)"/> operation is commutative but is not associative.<br/>
		/// The order of aggregation is important. For example, you may have attributes with DB boundnesses: <br/>
		/// <see cref="DbBoundnessType.DbBound"/>, <see cref="DbBoundnessType.PXDBScalar"/> and <see cref="DbBoundnessType.Unknown"/>.<br/><br/>
		/// Depending on the order of combination you may end up with either <see cref="DbBoundnessType.Error"/> or <see cref="DbBoundnessType.Unknown"/> DB boundness. <br/>
		/// We wish to end up with rather <see cref="DbBoundnessType.Error"/> than <see cref="DbBoundnessType.Unknown"/> and to achieve these with combination rules <br/>
		/// we should sort boundnesses in ascending order.
		/// </remarks>
		/// <param name="dbBoundnesses">The DB boundnesses to act on.</param>
		/// <returns>
		/// Combined DB Boundness.
		/// </returns>
		public static DbBoundnessType Combine(this IEnumerable<DbBoundnessType> dbBoundnesses)
		{
			var sortedDbBoundness = dbBoundnesses.CheckIfNull(nameof(dbBoundnesses)).OrderBy(b => b);
			DbBoundnessType aggregatedBoundness = DbBoundnessType.NotDefined;

			foreach (var dbBoundness in sortedDbBoundness)
			{
				aggregatedBoundness = aggregatedBoundness.Combine(dbBoundness);

				if (aggregatedBoundness == DbBoundnessType.Error)
					break;
			}

			return aggregatedBoundness;
		}

		public static DbBoundnessType Combine(this DbBoundnessType x, DbBoundnessType y)
		{
			// minor optimizations for frequent cases
			if (x == y || y == DbBoundnessType.NotDefined)
				return x;
			
			switch (x) 
			{
				case DbBoundnessType.NotDefined:
				case DbBoundnessType.Unbound:
					return y;		// y can't be NotDefined here

				case DbBoundnessType.DbBound:
					return y.CombineWithDbBound();

				case DbBoundnessType.PXDBScalar:
					return y.CombineWithPXDBCalcedOrDBScalar(isDbScalar: true);

				case DbBoundnessType.PXDBCalced:
					return y.CombineWithPXDBCalcedOrDBScalar(isDbScalar: false);

				case DbBoundnessType.Error:
					return x;

				case DbBoundnessType.Unknown:
				default:
					return y == DbBoundnessType.Error
						? DbBoundnessType.Error
						: DbBoundnessType.Unknown;
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

		private static DbBoundnessType CombineWithPXDBCalcedOrDBScalar(this DbBoundnessType y, bool isDbScalar)
		{
			switch (y)
			{
				case DbBoundnessType.DbBound:
				case DbBoundnessType.PXDBScalar:		// Multiple calced on DB side attributes should result in error
				case DbBoundnessType.PXDBCalced:
				case DbBoundnessType.Error:
					return DbBoundnessType.Error;

				case DbBoundnessType.Unknown:
					return DbBoundnessType.Unknown;

				case DbBoundnessType.Unbound:
				default:
					return isDbScalar
						? DbBoundnessType.PXDBScalar
						: DbBoundnessType.PXDBCalced;
			}
		}
	}
}