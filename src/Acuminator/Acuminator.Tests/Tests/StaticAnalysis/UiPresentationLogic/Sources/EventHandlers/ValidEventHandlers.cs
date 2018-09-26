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
		public PXAction<SOInvoice> Release;

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			PXUIFieldAttribute.SetDisplayName<SOInvoice.refNbr>(e.Cache, "Invoice Nbr.");
			PXUIFieldAttribute.SetVisible<SOInvoice.refNbr>(e.Cache, null, true);

			Release.SetVisible(true);
		}

		protected virtual void _(Events.CacheAttached<SOInvoice.refNbr> e)
		{
			PXUIFieldAttribute.SetEnabled<SOInvoice.refNbr>(e.Cache, null, false);
			PXUIFieldAttribute.SetRequired<SOInvoice.refNbr>(e.Cache, false);
			PXUIFieldAttribute.SetReadOnly<SOInvoice.refNbr>(e.Cache, null, true);

			Release.SetEnabled(false);
			Release.SetCaption("Release");
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