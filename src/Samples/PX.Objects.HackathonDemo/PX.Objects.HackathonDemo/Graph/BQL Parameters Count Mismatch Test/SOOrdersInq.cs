using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrdersInq : PXGraph<SOOrdersInq>
	{
		public PXSelect<SOOrder,
		   Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
			 And<SOOrder.status, Equal<Required<SOOrder.status>>>>,
		 OrderBy<
			 Asc<SOOrder.orderNbr>>> Orders;


		public SOOrderByTypeAndStatusSelect OrdersByStatus;

		#region Inheritance Test
		public SOOrder GetOrderByStatus()
		{
			var result = OrdersByStatus.SelectSingle("INV");  //We cannot determine required number of parameters for instances of derived BQL 

			if (result == null)
			{
				result = SOOrderByTypeAndStatusSelect.Select(this, "INV");  //But we can for static calls
			}

			return result;
		}
		#endregion

		#region Field Call Tests
		private IEnumerable GetOrders()
		{
			var result = Orders.Select();

			return result;
		}
		#endregion

		#region Static Calls Test
		public IEnumerable GetFilteredOrders(SOOrder order, bool useSimpeSelect, bool useDate)
		{
			PXSelectBase<SOOrder> filtered = new
					PXSelect<SOOrder,
					Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					  And<SOOrder.orderType, Greater<Required<SOOrder.orderType>>>>>(this);

			if (useSimpeSelect)
			{
				filtered = new PXSelect<SOOrder>(this);
			}

			var queryRes = filtered.Select(new[] { "SO00001" });  // no warning here due to if check

			if (queryRes.Count > 0)
				return queryRes;

			var parametersFirstSet = new[] { "SO00001", "INV" };
			var parametersSecondSet = new[] { "SO00001" };

            filtered = new
                PXSelect<SOOrder,
                    Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                      And<SOOrder.orderType, Greater<Required<SOOrder.orderType>>>>>(this);

			queryRes = filtered.Select(parametersFirstSet);

			if (queryRes.Count == 0)
				return queryRes;


            if (order.OrderDate.HasValue && filtered.Select(parametersSecondSet).Count > 0)
			{
				return null;
			}

			AddExtraCondition(filtered);  // no alerts after the modifying method
			return filtered.Select();
		}


		private void AddExtraCondition(PXSelectBase<SOOrder> query)
		{
			query.WhereAnd<Where<SOOrder.orderDate, Equal<SOOrder.orderDate>>>();
		}

		public static SOOrder[] GetSOOrders(SOOrdersInq graph, string orderType)
		{
			var result =
				PXSelect<SOOrder,
				Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
					And<SOOrder.status, Equal<Required<SOOrder.status>>>>>
				.SelectSingleBound(graph, pars: new[] { orderType }, currents: null)  //here parameters are passed via named argument
				.RowCast<SOOrder>()
				.ToArray();

			return result;
		}
		#endregion
	}
}
