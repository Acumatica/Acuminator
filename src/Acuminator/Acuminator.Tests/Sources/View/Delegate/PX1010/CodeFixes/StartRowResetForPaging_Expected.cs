using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LEPMaint : PXGraph<LEPMaint>
{
	public PXSelect<ListEntryPoint> Items1;

	public PXSelect<ListEntryPoint> Items2;
	public PXSelect<ListEntryPoint> Items3;

	public IEnumerable items1()
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
				PXView.StartRow = 0;
				return rows;
		}

		if (totalRows > 5)
		{
			PXView.StartRow = 0;
			return rows;
		}
		else
		{
			PXView.StartRow = 0;
			return rows;
		}

		PXView.StartRow = 0;
		return rows;
	}

	public IEnumerable items2()  //Should not apply code fix
	{
		int startRow = PXView.StartRow;
		int totalRows = 0;

		if (totalRows < 0)
			yield break;

		startRow = PXView.StartRow;

		IEnumerable<ListEntryPoint> rows = new PXView(this, false, new Select<ListEntryPoint>())
				.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
				ref startRow, PXView.MaximumRows, ref totalRows).Cast<ListEntryPoint>();

		foreach (var item in rows)
		{
			yield return item;
		}
	}

	public IEnumerable items3()  //Should not apply code fix
	{
		int startRow = PXView.StartRow;
		int totalRows = 0;

		startRow = PXView.StartRow;

		return new PXView(this, false, new Select<ListEntryPoint>())
				.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
				ref startRow, PXView.MaximumRows, ref totalRows).Cast<ListEntryPoint>();
	}
}