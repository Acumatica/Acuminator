using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.HackathonDemo.ViewOrder
{
	public class SOInvoiceEntry : ARInvoiceEntry
	{
		public PXSelect<SOInvoice> SOInvoices;  // There are two cache instances: PXCache<ARInvoice> and PXCache<SOInvoice>

		public PXSelect<ARTran> ARTrans;  // There is one cache : PXCache<SOTran>
	}
}
