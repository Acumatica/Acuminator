using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrder : IBqlTable
	{
		#region OrderNbr
		public abstract class orderNbr : IBqlField { }

		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr")]
		public int? OrderNbr { get; set; }
		#endregion

		#region OrderType
		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion
	}

	public class SOOrderWithTotal : PXCacheExtension<SOOrder>
	{
		#region Total
		public abstract class total : IBqlField { }

		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal Total { get; set; }
		#endregion
	}

	public class SOOrderWithHold : SOOrderWithTotal
	{
		#region Hold
		public abstract class hold : IBqlField { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold")]
		public bool? Hold { get; set; }
		#endregion
	}
}
