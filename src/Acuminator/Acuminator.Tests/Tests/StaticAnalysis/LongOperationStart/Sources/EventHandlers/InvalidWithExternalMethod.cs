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
			StartOperation();
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			StartOperation();
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			StartOperation();
		}

		private void StartOperation()
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