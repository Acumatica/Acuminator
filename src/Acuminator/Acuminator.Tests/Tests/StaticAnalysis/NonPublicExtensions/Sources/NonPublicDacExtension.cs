using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacUiAttributes.Sources
{ 
	internal sealed class SOOrderExt : PXCacheExtension<SOOrder>	//Non public DAC extensions are not supported
	{
        #region Total
        public abstract class total : IBqlField { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal? Total { get; set; }
		#endregion
	}

	sealed class SOOrderExtDiscount : PXCacheExtension<SOOrder>    //Non public DAC extensions are not supported
	{
		#region TotalDiscount
		public abstract class totalDiscount : IBqlField { }

		[PXDBDecimal]
		[PXUIField(DisplayName = "Total Discount")]
		public decimal? TotalDiscount { get; set; }
		#endregion
	}

	static class SalesGlobalState
	{
		private class SalesInfo
		{
			public sealed class SOOrderExtSales : PXCacheExtension<SOOrder>    //Non public DAC extensions are not supported
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
