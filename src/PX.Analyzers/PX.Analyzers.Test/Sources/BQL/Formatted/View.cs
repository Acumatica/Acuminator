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
		public PXSelect<
			SOOrder, 
			Where<SOOrder.orderType, Equal<SalesOrder>, 
				And<SOOrder.status, Equal<Open>>>, 
			OrderBy<
				Desc<SOOrder.orderType, 
				Asc<SOOrder.orderNbr, 
				Asc<SOOrder.status>>>>> 
			OpenSalesOrders;
	}
}
