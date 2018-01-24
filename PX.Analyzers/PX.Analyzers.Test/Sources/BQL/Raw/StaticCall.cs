using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{
		public PXSelect<SOOrder> OpenSalesOrders;

		public IEnumerable openSalesorders()
		{
			var result = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<SalesOrder>, And<SOOrder.status, Equal<Open>>>, OrderBy<Asc<SOOrder.orderNbr>>>.Select(this);
		}
	}
}
