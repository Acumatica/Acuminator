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
		protected virtual void ARInvoice_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			ARInvoice row = (ARInvoice)e.Row;
			if (row != null && !String.IsNullOrEmpty(row.DocType)
				&& !String.IsNullOrEmpty(row.RefNbr))
			{
				row.IsCCPayment = PaymentMethodHelper.IsCCPayment(row);
			}
		}
	}

	public static class PaymentMethodHelper
	{
		public static bool IsCCPayment(ARInvoice row)
		{
			return PXDatabase.SelectSingle<CustomerPaymentMethod>(
					   new PXDataField<CustomerPaymentMethod.cCProcessingCenterID>(),
					   new PXDataFieldRestrict<CustomerPaymentMethod.pMInstanceID>(row.pMInstanceID)) != null;
		}
	}
}