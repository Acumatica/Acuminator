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
			throw new PXException("Something bad happened");
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			throw new PXException("Something bad happened");
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			throw new PXException("Something bad happened");
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			throw new PXException("Something bad happened");
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			throw new PXException("Something bad happened");
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			throw new PXException("Something bad happened");
		}

		protected virtual void _(Events.RowInserted<SOInvoice> e)
		{
			throw new NotSupportedException();
		}

		protected virtual void _(Events.RowUpdated<SOInvoice> e)
		{
			throw new ArgumentException("Something bad happened");
		}

		protected virtual void _(Events.RowDeleted<SOInvoice> e)
		{
			throw new Exception("Something bad happened");
		}

		protected virtual void _(Events.RowPersisting<SOInvoice> e) => throw new ArgumentNullException();

		protected virtual void _(Events.RowSelected<SOInvoice> e) =>
			throw new InvalidOperationException("Something bad happened");
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