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
			Actions.PressSave();
		}
	}

	public class SOInvoice : IBqlTable
	{
	}
}