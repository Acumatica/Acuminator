using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.HackathonDemo
{
	public partial class APInvoiceEntry : PXGraph<APInvoiceEntry>
	{
		public IEnumerable currentOrder()
		{
			return Enumerable.Empty<SOOrder>();
		}

		public IEnumerable voidInvoice(PXAdapter adapter)
		{
			yield break;
		}

		public void viewBatch()
		{
			
		}
	}
}