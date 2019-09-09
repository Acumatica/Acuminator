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

		public PXSelect<SOOrder,
				 Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				   And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>> View2;

		public object TestVariableSearchCall(SOOrder order)
		{
			// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters [Justification]
			var result1 = View1.Search<SOOrder.orderNbr>(order.OrderNbr, order.OrderDate); //diagnostic here
			var result2 = View1.Search<SOOrder.orderNbr>(order.OrderNbr); //no diagnostic 
																		  // Acuminator disable once PX1015 IncorrectNumberOfSelectParameters [Justification]
			var result3 = View2.Search<SOOrder.orderNbr, SOOrder.status>(order.OrderNbr, order.Status);  //diagnostic here
			return result1;
		}

		public object TestStaticSearchCall(SOOrder order)
		{
			// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters [Justification]
			var result1 =
				PXSelect<SOOrder,
				 Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
				   And<SOOrder.orderDate, Greater<Current<SOOrder.orderDate>>>>>
				 .Search<SOOrder.orderNbr>(this, order.OrderNbr, order.OrderDate); //diagnostic here

			var result2 =
				PXSelect<SOOrder,
				   Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					 And<SOOrder.orderDate, Greater<Current<SOOrder.orderDate>>>>>
				.Search<SOOrder.orderNbr>(this, order.OrderNbr);                            //no diagnostic 

			// Acuminator disable once PX1015 IncorrectNumberOfSelectParameters [Justification]
			var result3 =
				PXSelect<SOOrder,
				   Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					 And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>
				.Search<SOOrder.orderNbr, SOOrder.status>(this, order.OrderNbr, order.Status);  //diagnostic here
			return result1;
		}
	}
}
