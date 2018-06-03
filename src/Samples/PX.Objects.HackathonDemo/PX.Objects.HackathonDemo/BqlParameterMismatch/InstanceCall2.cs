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

            if (flag)
                filtered = new PXSelect<SOOrder>(this);
            else
                filtered = new
                PXSelect<SOOrder,
                    Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                      And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

            var p1 = new[] { 1, 2 };
			var p2 = new[] { 1 };

            filtered = new
                PXSelect<SOOrder,
                    Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                      And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

            if (order.OrderDate.HasValue && filtered.Select(p2).Count > 0)
			{
				
			}

			return this;
		}


		public void Test(PXSelectBase<SOOrder> query)
		{

		}
	}
}
