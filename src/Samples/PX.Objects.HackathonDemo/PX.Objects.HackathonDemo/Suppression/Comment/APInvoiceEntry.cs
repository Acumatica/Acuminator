using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.HackathonDemo.Suppression.Comments
{
	public partial class APInvoiceEntry : PXGraph<APInvoiceEntry>
	{
		#region Views
		public PXSelect<ListEntryPoint> Items;

		public PXSelect<APInvoice> Orders;

		public PXSetup<APSetup> APSetup;

		public PXSelect<TaxTran> Taxes;

		#region Current Order
		public PXSelect<APInvoice,
					Where<APInvoice.refNbr, Equal<Current<APInvoice.refNbr>>,
						And<APInvoice.docType, Equal<Current<APInvoice.docType>>>>> CurrentOrder;
		#endregion
		#endregion

		public PXAction<APInvoice> Release;

		public PXAction<APInvoice> ViewBatch;

		public PXAction<APInvoice> VoidInvoice;


		protected virtual void APInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			APInvoice doc = e.Row as APInvoice;
			if (doc == null) return;

			if (doc.Released != true)
			{
				if (doc.Released != true)
				{
					if (true)
					{
						if (doc.CuryDocBal != doc.CuryOrigDocAmt)
						{
							if (doc.CuryDocBal != null && doc.CuryDocBal != 0)
								sender.SetValueExt<APInvoice.curyOrigDocAmt>(doc, doc.CuryDocBal);
							else
								sender.SetValueExt<APInvoice.curyOrigDocAmt>(doc, 0m);
						}
					}

					if (doc.DocType == "PPM")
					{
						//sender.SetValue<APInvoice.dueDate>(e.Row, this.Accessinfo.BusinessDate);
					}
				}

				if (doc.Hold != true && doc.Released != true)
				{
					if (doc.CuryDocBal != doc.CuryOrigDocAmt)
					{
						sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXException());
					}
					else if (doc.CuryOrigDocAmt < 0m)
					{
						if (APSetup.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, doc.CuryOrigDocAmt, new PXException());
						}
						else
						{
							sender.RaiseExceptionHandling<APInvoice.curyDocBal>(doc, doc.CuryDocBal, new PXException());
						}
					}
					else
					{
						if (APSetup.Current.RequireControlTotal == true)
						{
							sender.RaiseExceptionHandling<APInvoice.curyOrigDocAmt>(doc, null, null);
						}
						else
						{
							sender.RaiseExceptionHandling<APInvoice.curyDocBal>(doc, null, null);
						}
					}
				}
			}

			bool checkControlTaxTotal = APSetup.Current.RequireControlTaxTotal == true;

			if (doc.Hold != true && doc.Released != true)
			{
				throw new PXException();
			}
		}

		protected virtual void APInvoice_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
		}

		protected virtual void APInvoice_DocDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		#region Delegate
		public IEnumerable items()
		{
			int startRow = PXView.StartRow;
			int totalRows = 0;

			startRow = PXView.StartRow;

			// Acuminator disable once PX1010 StartRowResetForPaging [Justification]
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

		// Acuminator disable once PX1092 IncorrectAttributesOnActionHandler [Justification]
		public IEnumerable release(PXAdapter adapter)
		{
			return adapter.Get();
		}
	}
}