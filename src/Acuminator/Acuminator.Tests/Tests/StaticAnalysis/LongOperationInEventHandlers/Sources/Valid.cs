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

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			e.Cancel = e.PXLongOperation.Exists(this.UID);
		}

		protected virtual void release()
		{
			PXLongOperation.StartOperation(this, null);
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