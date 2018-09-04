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
			InsertNewRecord();
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice.refNbr> e)
		{
			InsertNewRecord();
		}

		protected virtual void _(Events.RowSelecting<SOInvoice> e)
		{
			InsertNewRecord();
		}

		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			InsertNewRecord();
		}

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			InsertNewRecord();
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			InsertNewRecord();
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			InsertNewRecord();
		}

		private void InsertNewRecord()
		{
			var cache = this.Caches[typeof (SOInvoice)];
			var row = (SOInvoice) cache.Insert();
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