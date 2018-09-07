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
		protected virtual void SOInvoice_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			
		}

		protected virtual void SOInvoice_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{

		}

		protected virtual void SOInvoice_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{

		}

		protected virtual void SOInvoice_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{

		}

		protected virtual void SOInvoice_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{

		}

		protected virtual void SOInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{

		}

		protected virtual void SOInvoice_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{

		}

		protected virtual void SOInvoice_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{

		}

		protected virtual void SOInvoice_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{

		}

		protected virtual void SOInvoice_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{

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