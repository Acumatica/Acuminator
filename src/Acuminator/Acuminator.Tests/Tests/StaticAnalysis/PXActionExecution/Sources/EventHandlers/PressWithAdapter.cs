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

		protected virtual void _(Events.FieldDefaulting<SOInvoice, SOInvoice.refNbr> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			Release.Press(null);
		}

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			Release.Press(null);
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