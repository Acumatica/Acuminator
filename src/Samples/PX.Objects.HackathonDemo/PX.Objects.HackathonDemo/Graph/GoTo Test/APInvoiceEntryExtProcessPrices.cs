using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.HackathonDemo
{
	public partial class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
	{
		public IEnumerable currentOrderExtended()
		{
			return Enumerable.Empty<SOOrder>();
		}

		public void processPrices()
		{
			
		}

		public IEnumerable taxes()
		{
			return Enumerable.Empty<TaxTran>();
		}
	}
}