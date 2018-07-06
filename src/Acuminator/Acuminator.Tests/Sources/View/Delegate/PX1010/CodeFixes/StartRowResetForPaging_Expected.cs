using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LEPMaint : PXGraph<LEPMaint>
{
	public PXSelect<ListEntryPoint> Items;

	public IEnumerable items()
	{
		int startRow = PXView.StartRow;
		int totalRows = 0;

		startRow = PXView.StartRow;

		IEnumerable<ListEntryPoint> rows = new PXView(this, false, new Select<ListEntryPoint>())
				.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
				ref startRow, PXView.MaximumRows, ref totalRows).Cast<ListEntryPoint>();
		PXView.StartRow = 0;
		return rows;
	}
}