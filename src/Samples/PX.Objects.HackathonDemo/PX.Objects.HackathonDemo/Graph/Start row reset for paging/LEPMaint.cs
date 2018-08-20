using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.HackathonDemo
{
	public partial class LEPMaint : PXGraph<LEPMaint>
	{
		public PXSelect<ListEntryPoint> Items;

		public PXSelect<SOOrder> Orders;

		public IEnumerable items()
		{
			int startRow = PXView.StartRow;
			int totalRows = 0;

			startRow = PXView.StartRow;

			IEnumerable<ListEntryPoint> rows = new PXView(this, false, new Select<ListEntryPoint>())
					.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
					ref startRow, PXView.MaximumRows, ref totalRows).Cast<ListEntryPoint>();

			//example of code fix generation of start row reset before each return statement
			switch (totalRows)
			{
				case 3:
					return rows;
			}

			if (totalRows < 5)
				return rows;
			else if (totalRows > 1)
				return rows;

			return rows;
		}
	}
}