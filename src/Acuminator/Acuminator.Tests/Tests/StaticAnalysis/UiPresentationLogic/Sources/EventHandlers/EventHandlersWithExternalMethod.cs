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
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			InsertNewRecord(e.Cache);
		}

		private void InsertNewRecord(PXCache cache)
		{
			PXUIFieldAttribute.SetVisible(cache, nameof (SOInvoice.RefNbr), true);
			PXUIFieldAttribute.SetEnabled<SOInvoice.refNbr>(cache, null, false);
			PXUIFieldAttribute.SetDisplayName<SOInvoice.refNbr>(cache, "Invoice Nbr.");
			PXUIFieldAttribute.SetRequired<SOInvoice.refNbr>(e.Cache, false);
			PXUIFieldAttribute.SetReadOnly<SOInvoice.refNbr>(e.Cache, null, true);

			Release.SetVisible(true);
			Release.SetEnabled(false);
			Release.SetCaption("Release");
			Release.SetTooltip("Release the document");
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