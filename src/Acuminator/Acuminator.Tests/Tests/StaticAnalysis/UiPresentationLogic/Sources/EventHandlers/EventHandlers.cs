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
			PXUIFieldAttribute.SetVisible(cache, nameof (SOInvoice.RefNbr), true);
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			PXUIFieldAttribute.SetEnabled<SOInvoice.refNbr>(cache, null, false);
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			PXUIFieldAttribute.SetDisplayName<SOInvoice.refNbr>(cache, "Invoice Nbr.");
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			PXUIFieldAttribute.SetRequired<SOInvoice.refNbr>(e.Cache, false);
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			PXUIFieldAttribute.SetReadOnly<SOInvoice.refNbr>(e.Cache, null, true);
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			PXUIFieldAttribute.SetNeutralDisplayName(e.Cache, nameof (SOInvoice.RefNbr), "Invoice Nbr.");
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			PXUIFieldAttribute.SetVisibility<SOInvoice.refNbr>(cache, null, PXUIVisibility.HiddenByAccessRights);
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			Release.SetVisible(true);
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			Release.SetEnabled(false);
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{
			Release.SetCaption("Release");
		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			Release.SetTooltip("Release the document");
			PXStringListAttribute.SetList<SOInvoice.refNbr>(e.Cache, null, new [] { "1" }, new [] { "0001" });
			PXIntListAttribute.SetList<SOInvoice.refNbr>(e.Cache, null, new [] { 1 }, new [] { "0001" });
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