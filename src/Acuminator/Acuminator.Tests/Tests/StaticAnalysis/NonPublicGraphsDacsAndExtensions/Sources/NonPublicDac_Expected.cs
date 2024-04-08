using System;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicGraphsDacsAndExtensions.Sources
{
	public sealed class SOOrder : IBqlTable
	{
		#region Total
		public abstract class total : IBqlField { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal? Total { get; set; }
		#endregion
	}

	public sealed class SOOrderDiscount : IBqlTable
	{
		#region TotalDiscount
		public abstract class totalDiscount : IBqlField { }

		[PXDBDecimal]
		[PXUIField(DisplayName = "Total Discount")]
		public decimal? TotalDiscount { get; set; }
		#endregion
	}

	public static class SalesGlobalState
	{
		public class SalesInfo
		{
			public class SOOrderExtSales1 : IBqlTable
			{
				#region TotalSales
				public abstract class totalSales : IBqlField { }

				[PXDBDecimal]
				[PXUIField(DisplayName = "Total Sales")]

				public decimal? TotalSales { get; set; }
				#endregion
			}

			public sealed class SOOrderExtSales2 : IBqlTable
			{
				#region TotalSales
				public abstract class totalSales : IBqlField { }

				[PXDBDecimal]
				[PXUIField(DisplayName = "Total Sales")]

				public decimal? TotalSales { get; set; }
				#endregion
			}
		}
	}
}