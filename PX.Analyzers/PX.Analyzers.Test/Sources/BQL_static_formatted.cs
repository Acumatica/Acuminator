using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.SO
{
	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{
		public PXSelect<
			SOOrder> 
			OpenSalesOrders;

		public IEnumerable openSalesorders()
		{
			PXSelect<
				SOOrder, 
				Where<SOOrder.orderType, Equal<SalesOrder>, And<SOOrder.status, Equal<Open>>>, 
				OrderBy<
					Asc<SOOrder.orderNbr>>>
				.Select(this);
		}
	}

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
		public string Status { get; set; }
		#endregion
	}

	public class Open : Constant<string>
	{
		public Open() : base("O")
		{
		}
	}

	public class SalesOrder : Constant<string>
	{
		public SalesOrder() : base("SO")
		{
		}
	}
}
