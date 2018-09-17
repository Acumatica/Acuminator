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
			Persist();
		}
	}

	public class SOInvoice : IBqlTable
	{
	}
}