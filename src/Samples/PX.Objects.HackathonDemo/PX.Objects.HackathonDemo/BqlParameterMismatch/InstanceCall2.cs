using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderTestEntry3 : PXGraph<SOOrderTestEntry3>
	{
		public object Foo(SOOrder order, bool flag)
		{
			PXSelectBase<SOOrder> filtered = new
					PXSelect<SOOrder,
					Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					  And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

			if (false)
				filtered = new PXSelect<SOOrder>(this);



            filtered = new
                PXSelect<SOOrder,
                    Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                      And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);


            filtered?.View?.WhereAnd(typeof(object));

            if (order.OrderDate.HasValue && filtered.Select(order.OrderDate).Count > 0)
			{
				
			}
			return this;
		}


		public void Test(PXSelectBase<SOOrder> query)
		{

		}
	}
}
