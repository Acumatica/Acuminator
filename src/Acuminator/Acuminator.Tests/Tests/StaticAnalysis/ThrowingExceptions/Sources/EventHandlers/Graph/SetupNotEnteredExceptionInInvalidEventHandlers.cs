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
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		// Check expressions thrown as throw expression here
		public void SOInvoice_RefNbr_ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e) =>
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public void SOInvoice_RefNbr_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e) =>
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public void SOInvoice_RefNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e) =>
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public void SOInvoice_RefNbr_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e) =>
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public void SOInvoice_RefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) =>
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public PXSelect<SOInvoice> Invoices;
	}

	[PXHidden]
	public class SOInvoice : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion	
	}
}