using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOTran : IBqlTable
	{
		#region OrderNbr
		public abstract class orderNbr : IBqlField { }
		[PXDBString]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]

		public string OrderNbr { get; set; }
		#endregion
	}

	public class SOOrderUpdateEntry : PXGraph<SOOrderUpdateEntry>
	{
		public void TestUpdateCall(SOOrder order)
		{
			PXUpdate<
					Set<SOOrder.status, Required<SOOrder.status>,
					Set<SOOrder.orderType, Null>>,
					SOOrder,
					Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						And<SOOrder.orderType, IsNotNull,
						And<SOOrder.orderDate, Equal<Required<SOOrder.orderDate>>>>>>
					.Update(this, order.Status, order.OrderNbr, order.OrderDate);           //No diagnostic here


			// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters [Justification]
			PXUpdate<
					Set<SOOrder.status, Required<SOOrder.status>,
					Set<SOOrder.orderType, Null>>,
					SOOrder,
					Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						And<SOOrder.orderType, IsNotNull,
						And<SOOrder.orderDate, Equal<Required<SOOrder.orderDate>>>>>>
					.Update(this, order.Status, order.OrderDate);           //diagnostic here

			// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters [Justification]
			PXUpdateJoin<
					Set<SOOrder.status, Required<SOOrder.status>,
					Set<SOOrder.orderType, Null>>,
					SOOrder,
					InnerJoin<SOTran,
						On<SOOrder.orderNbr, Equal<SOTran.orderNbr>>>,
					Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
						And<SOOrder.orderType, IsNotNull,
						And<SOOrder.orderDate, Equal<Required<SOOrder.orderDate>>>>>>
					.Update(this, order.Status, order.OrderDate);           //diagnostic here

		}
	}
}
