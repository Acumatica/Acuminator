using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo.Extensions.NonPublic
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
}
