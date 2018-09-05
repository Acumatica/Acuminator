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
		protected virtual void ARInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			sender.Persist(e.Row, PXDBOperation.Normal);

			sender.PersistInserted(e.Row);
			sender.PersistUpdated(e.Row);
			sender.PersistDeleted(e.Row);
		}
	}

	public class SOInvoice : IBqlTable
	{
	}
}