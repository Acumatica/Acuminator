using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderTestEntry1 : PXGraph<SOOrderTestEntry1>
	{
		PXSelect<SOOrder,
		   Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
			 And<SOOrder.status, Equal<Required<SOOrder.status>>>>,
		 OrderBy<
			 Asc<SOOrder.orderNbr>>> select;

		public object Foo()
		{
			var result = select.SelectSingle();

            return this;
		}
	}
}
