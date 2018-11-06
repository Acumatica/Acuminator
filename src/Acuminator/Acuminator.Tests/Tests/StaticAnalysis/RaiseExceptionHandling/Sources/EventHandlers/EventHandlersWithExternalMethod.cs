using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void _(Events.FieldDefaulting<SOInvoice, SOInvoice.refNbr> e)
		{
			ShowErrors(e.Cache);
		}
		
		protected virtual void _(Events.FieldSelecting<SOInvoice.refNbr> e)
		{
			ShowErrors(e.Cache);
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			ShowErrors(e.Cache);
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			ShowErrors(e.Cache);
		}

		private void ShowErrors(PXCache cache)
		{
			cache.RaiseExceptionHandling<SOInvoice.refNbr>(null, null, new PXSetPropertyException("Something bad happened"));
		}
	}

	public class SOInvoice : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion	
	}
}