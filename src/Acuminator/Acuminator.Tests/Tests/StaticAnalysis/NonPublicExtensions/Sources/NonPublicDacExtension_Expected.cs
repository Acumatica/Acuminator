using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicExtensions.Sources
{
	public sealed class SOOrderExt : PXCacheExtension<SOOrder>
	{
		#region Total
		public abstract class total : IBqlField { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal? Total { get; set; }
		#endregion
	}

	public sealed class SOOrderExtDiscount : PXCacheExtension<SOOrder>
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
			public sealed class SOOrderExtSales1 : PXCacheExtension<SOOrder>
			{
				#region TotalSales
				public abstract class totalSales : IBqlField { }

				[PXDBDecimal]
				[PXUIField(DisplayName = "Total Sales")]

				public decimal? TotalSales { get; set; }
				#endregion
			}

			public sealed class SOOrderExtSales2 : PXCacheExtension<SOOrder>
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



	[PXHidden]
	public class SOOrder : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : IBqlField { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXUIField(DisplayName = "Status")]
		[PXString]
		public string Status { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
