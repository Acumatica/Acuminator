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
			ExecuteRelease();
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			ExecuteRelease();
		}

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			ExecuteRelease();
		}

		private void ExecuteRelease(PXCache cache)
		{
			Release.Press();
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