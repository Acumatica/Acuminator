using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrder : IBqlTable
	{
		public abstract class orderType : IBqlField { }
		public abstract class module : IBqlField { }
	}

	public class SOOrderTestEntry : PXGraph<SOOrderTestEntry>
	{
		public object Test_Static_Call_With_Custom_Predicate()
		{
			var args = new string[] { "RCT", "PO" };

			var result =
                PXSelect<SOOrder, 
				Where2<
					AreSame<SOOrder.orderType, Required<SOOrder.orderType>>,
				  And<
					  AreDistinct<SOOrder.module, Required<SOOrder.module>>>>>
				.SelectSingleBound(this, pars: args, currents: null)
				.ToArray();
			
            return this;
		}
	}
}
