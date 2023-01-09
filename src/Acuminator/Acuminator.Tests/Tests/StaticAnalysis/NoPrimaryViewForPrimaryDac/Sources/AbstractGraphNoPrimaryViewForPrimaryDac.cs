using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	[PXCacheName("ARInvoice")]
	public class ARInvoice : IBqlTable { }

	public abstract class InvoiceEntryBase : PXGraph<InvoiceEntryBase, ARInvoice>
	{
	}

	public class ARInvoiceEntry : InvoiceEntryBase
	{
		public PXSelect<ARInvoice> Invoices;
	}

	public class SOInvoiceEntry : ARInvoiceEntry
	{

	}
}