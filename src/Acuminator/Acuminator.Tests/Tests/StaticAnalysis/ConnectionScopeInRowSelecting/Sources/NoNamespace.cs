using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects
{
	public class SOInvoiceEntry : ARInvoiceEntry
	{
		protected virtual void ARInvoice_RowSelecting(PX.Data.PXCache sender, PX.Data.PXRowSelectingEventArgs e)
		{
			ARInvoice row = (ARInvoice)e.Row;
			if (row != null && !String.IsNullOrEmpty(row.DocType)
				&& !String.IsNullOrEmpty(row.RefNbr))
			{
				row.IsCCPayment = false;
				if (PX.Data.PXSelect<CustomerPaymentMethodC>.Select(this, row.DocType, row.RefNbr).Count > 0)
				{
					row.IsCCPayment = true;
				}
			}
		}
	}
}