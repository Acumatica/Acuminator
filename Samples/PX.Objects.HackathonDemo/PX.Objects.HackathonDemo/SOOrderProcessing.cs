using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderProcessing : PXGraph<SOOrderProcessing>
	{
		public class SOSelectedOrder : SOOrder
		{
			public abstract class selected : IBqlField { }
			[PXBool]
			[PXUIField(DisplayName = "Selected")]
			public bool? Selected { get; set; }
		}

		public PXProcessing<SOSelectedOrder> Orders;
		public PXSelect<SOOrder> CurrentOrder;

		public SOOrderProcessing()
		{
			Orders.SetProcessDelegate(item =>
			{
				this.PerformRelease(item);
			});
		}

		public void PerformRelease(SOOrder order)
		{
			var setup = PXSelect<SOSetup>.SelectSingleBound(new PXGraph(), null);
			// some release logic
		}
	}
}
