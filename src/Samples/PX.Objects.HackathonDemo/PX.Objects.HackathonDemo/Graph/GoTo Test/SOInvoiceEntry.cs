using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.HackathonDemo.DerivedGraph
{
	public class SOInvoiceEntry : APInvoiceEntry
	{
		public override string Module => "SO";

		#region Views		
		public PXSelect<SOInvoice> Invoices;

		#region Current Invoice
		public PXSelect<SOInvoice,
					Where<SOInvoice.refNbr, Equal<Current<SOInvoice.refNbr>>>> CurrentInvoice;
		#endregion
		#endregion

		public PXAction<SOInvoice> Void;
	
        protected override void _(Events.RowInserting<APInvoice> e)
        {
        }

		#region Delegate
		public IEnumerable taxes()
		{
			return PXSelect<TaxTran>.Select(this);
		}
		#endregion
	}
}