using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class CustomSelect : PXSelect<SOOrder,
		   Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
			 And<SOOrder.status, Equal<Required<SOOrder.status>>>>,
		 OrderBy<
			 Asc<SOOrder.orderNbr>>>
	{
		public CustomSelect(PXGraph graph) : base(graph)
		{
		}
	}

	public class SOOrderTestEntry2 : PXGraph<SOOrderTestEntry2>
	{
		CustomSelect select;

		public object Foo()
		{
			var result = select.SelectSingle(1);

			return this;
		}
	}
}
