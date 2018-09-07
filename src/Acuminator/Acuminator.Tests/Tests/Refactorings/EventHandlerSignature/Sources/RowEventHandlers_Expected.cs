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
		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{

		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{

		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{

		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{

		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{

		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{

		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{

		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e)
		{

		}

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
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