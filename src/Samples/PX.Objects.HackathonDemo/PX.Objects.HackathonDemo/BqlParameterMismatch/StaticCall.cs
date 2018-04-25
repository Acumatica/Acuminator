using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderTestEntry : PXGraph<SOOrderTestEntry>
	{
		public object Foo()
		{
			var result =
                PXSelect<SOOrder, 
				Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>, 
					And<SOOrder.status, Equal<Required<SOOrder.status>>>>>
				.SelectSingleBound(this, pars: new[] { 2 }, currents: null).ToArray();
			
            return this;
		}
	}
}
