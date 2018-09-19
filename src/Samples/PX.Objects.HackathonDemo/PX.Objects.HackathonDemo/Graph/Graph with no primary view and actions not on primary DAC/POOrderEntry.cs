using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class POOrderEntry : PXGraph<POOrderEntry, POOrder>
	{
		public PXSelect<SOOrder> Orders;
		
		public PXAction<SOOrder> Release;
	}

	public class POOrderEntryExt : PXGraphExtension<POOrderEntry>
	{
		public PXAction<SOOrder> RefreshOrder;
	}
}
