using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{
		public PXSelect<SOOrder> Orders;
		public PXAction<SOOrder> Release;

		public virtual IEnumerable odrers()
		{
			int startRow = PXView.StartRow;
			var rows = PXSelect<SOOrder, Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>
				.Select(this, startRow, PXView.MaximumRows);
			foreach (var row in rows)
			{
				SOOrder order = new SOOrder();
				order.OrderType = row.GetItem<SOOrder>().OrderType;
				order.OrderNbr = row.GetItem<SOOrder>().OrderNbr;
			}
			return rows;
		}

		[PXButton]
		[PXUIField(DisplayName = "Release")]
		public void release(PXAdapter adapter)
		{
			// release method implementation
			var graph = new SOOrderProcessing();
		}
	}
}
