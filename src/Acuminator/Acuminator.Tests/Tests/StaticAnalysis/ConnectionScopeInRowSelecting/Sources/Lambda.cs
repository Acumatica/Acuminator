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
		protected virtual void ARInvoice_CacheAttached(PXCache cache)
		{
			cache.Graph.RowSelecting.AddHandler<ARInvoice>((sender, e) => PXDatabase.SelectTimeStamp());
		}
	}
}