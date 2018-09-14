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
		protected virtual void _(Events.FieldDefaulting<SOInvoice.refNbr> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			PXLongOperation.StartOperation(this, null);
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
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