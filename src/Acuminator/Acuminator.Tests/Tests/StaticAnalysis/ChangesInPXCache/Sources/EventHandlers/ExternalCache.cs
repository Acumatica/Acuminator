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
		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			this.Caches[typeof(Customer)].Insert();
		}

		protected virtual void _(Events.RowUpdating<SOInvoice> e)
		{
			this.Caches[typeof(Customer)].Insert();
		}

		protected virtual void _(Events.RowDeleting<SOInvoice> e)
		{
			this.Caches[typeof(Customer)].Insert();
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

	public class Customer : IBqlTable
	{
	}
}