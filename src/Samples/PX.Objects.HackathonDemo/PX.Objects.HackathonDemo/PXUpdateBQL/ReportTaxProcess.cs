using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class ReportTaxProcess : PXGraph<ReportTaxProcess>
	{
		public PXSelect<SOOrder> Orders;
		public PXAction<SOOrder> Release;

		public virtual void VoidReportProc()
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PXUpdate<
						Set<SOOrder.orderType, Null,
							Set<SOOrder.status, Null>>,
						SOOrder,
						Where<SOOrder.orderDate, Equal<Required<SOOrder.orderDate>>,
							And<SOOrder.orderType, NotEqual<Null>>>>
						.Update(this, DateTime.Now);

					ts.Complete(this);
				}
			}
		}
	}
}
