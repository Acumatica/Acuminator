using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrder_With_Total : PXCacheExtension<SOOrder>
	{
		#region Total_Amount
		public abstract class total_Amount : IBqlField { }

		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal Total_Amount { get; set; }
		#endregion

		#region Test
		public abstract class __ : IBqlField { }

		[PXDBDecimal]
		public decimal __ { get; set; }
		#endregion
	}
}
