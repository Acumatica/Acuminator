using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry>
	{
		public PXCancel<SOInvoice> Cancel;

		protected virtual void _(Events.FieldDefaulting<SOInvoice, SOInvoice.refNbr> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			Cancel.Press();
		}

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			Cancel.Press();
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