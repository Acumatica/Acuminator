using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrder : IBqlTable { }

	public class SOTran : IBqlTable { }

	public class BaseGraph : PXGraph<BaseGraph, SOOrder>
	{
		public PXSelect<SOTran> OrderDetails;

		public PXSelect<SOOrder> Orders;
	}

	public class DerivedGraph : BaseGraph
	{
		public PXSelect<SOTran> MoreOrderDetails;

		public PXAction<SOTran> Release1;
	}
}
