using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderWithTotal : PXCacheExtension<SOOrder>
	{
		#region Total_Amount
		public abstract class totalAmount : IBqlField { }

		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal TotalAmount { get; set; }
		#endregion

		#region Test
		public abstract class __ : IBqlField { }

		[PXDBDecimal]
		public decimal __ { get; set; }
		#endregion
	}
}
