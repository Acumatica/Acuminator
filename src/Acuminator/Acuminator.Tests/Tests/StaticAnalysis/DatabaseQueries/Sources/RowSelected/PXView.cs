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
		public PXSelectJoin<CustomerPaymentMethodC,
			InnerJoin<CA.PaymentMethod,
				On<CA.PaymentMethod.paymentMethodID,
					Equal<CustomerPaymentMethodC.paymentMethodID>>,
				InnerJoin<SOInvoice,
					On<SOInvoice.pMInstanceID,
						Equal<CustomerPaymentMethodC.pMInstanceID>>>>,
			Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>,
				And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>,
					And<CA.PaymentMethod.paymentType,
						Equal<CA.PaymentMethodType.creditCard>,
						And<CA.PaymentMethod.aRIsProcessingRequired, Equal<True>>>>>> CCPayments;

		protected virtual void ARInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARInvoice row = (ARInvoice)e.Row;
			if (row != null && !String.IsNullOrEmpty(row.DocType)
				&& !String.IsNullOrEmpty(row.RefNbr))
			{
				row.IsCCPayment = false;
				if (CCPayments.View.SelectMulti(row.DocType, row.RefNbr).Count > 0)
				{
					row.IsCCPayment = true;
				}
			}
		}
	}
}