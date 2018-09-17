using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void ARInvoice_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.PersistInserted(e.Row);
		}

		protected virtual void ARInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.PersistUpdated(e.Row);
		}

		protected virtual void ARInvoice_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.PersistDeleted(e.Row);
		}

		protected virtual void ARInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			sender.Persist(e.Row, PXDBOperation.Insert);
			sender.Persist(e.Row, PXDBOperation.Update);
			sender.Persist(e.Row, PXDBOperation.Delete);
		}
	}

	public class SOInvoice : IBqlTable
	{
	}
}