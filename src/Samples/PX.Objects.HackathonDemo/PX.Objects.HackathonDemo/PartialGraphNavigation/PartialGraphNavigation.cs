using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.HackathonDemo
{
	public partial class SimpleOrdersMaint : PXGraph<LEPMaint>
	{
		public PXSelect<ListEntryPoint> Items;

		public PXSelect<SOOrder> Orders;


		public PXSelect<SOOrder,
					Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>> CurrentOrder;


		public PXAction<SOOrder> Release;

		public PXAction<SOOrder> VoidOrder;










































		//public IEnumerable voidOrder(PXAdapter adapter)
		//{
		//	yield break;
		//}










		#region Delegate
		public IEnumerable items()
		{
			int startRow = PXView.StartRow;
			int totalRows = 0;

			startRow = PXView.StartRow;

			IEnumerable<ListEntryPoint> rows = new PXView(this, false, new Select<ListEntryPoint>())
					.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
					ref startRow, PXView.MaximumRows, ref totalRows).Cast<ListEntryPoint>();

			switch (totalRows)
			{
				case 3:
					return rows;
			}

			if (totalRows < 5)
				return rows;
			else
				return rows;

			return rows;
		}
		#endregion

		public IEnumerable release(PXAdapter adapter)
		{
			yield break;
		}

		public abstract void orders();
	}
}