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
			sender.RaiseExceptionHandling("RefNbr", e.Row, null, new PXSetPropertyException("Something bad happened"));
		}
	}

	public class SOInvoice : IBqlTable
	{
	}
}