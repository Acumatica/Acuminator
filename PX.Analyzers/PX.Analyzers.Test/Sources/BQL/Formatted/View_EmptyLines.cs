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
		public PXCancel<SOOrder> Cancel;




		public PXSelect<
			SOOrder, 
			Where<SOOrder.orderType, Equal<SalesOrder>>> 
			Orders;
	}
}
