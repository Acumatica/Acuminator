using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderEntry4 : PXGraph<SOOrderEntry4>
	{
		public PXSelect<SOOrder,
				 Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				   And<SOOrder.orderDate, Greater<Current<SOOrder.orderDate>>>>> View1;

		public object TestVariableSearchCall(SOOrder order)
		{
			var result1 = View1.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderDate); //diagnostic here
			var result2 = View1.Search<SOOrder.orderNbr>(order.OrderNbr); //no diagnostic 
			return result1;
		}
	}
}
