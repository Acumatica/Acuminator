using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : ARInvoiceEntry
	{
		protected virtual void ARInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARInvoice row = (ARInvoice)e.Row;
			if (row != null && !String.IsNullOrEmpty(row.DocType)
				&& !String.IsNullOrEmpty(row.RefNbr))
			{
				row.IsCCPayment = false;
				if (PaymentMethodHelper.IsCCPayment(
					PXSelectorAttribute.Select<ARInvoice.paymentMethodID>(sender, row)))
				{
					row.IsCCPayment = true;
				}
			}
		}
	}
}