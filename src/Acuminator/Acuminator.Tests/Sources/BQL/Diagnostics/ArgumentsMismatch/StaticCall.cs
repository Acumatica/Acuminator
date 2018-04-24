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
		public object Foo()
		{
			var result =
                PXSelect<SOOrder, 
				Where<SOOrder.orderType, Equal<Required<SalesOrder.orderType>>, 
					And<SOOrder.status, Equal<Required<SalesOrder.status>>>>, 
				OrderBy<
					Asc<SOOrder.orderNbr>>>
				.Select(this);

            return this;
		}
	}
}
