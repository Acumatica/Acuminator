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
			SOInvoice row = null;
			row = e.Row;
			e.Cache.SetValue<SOInvoice.refNbr>(row, "<NEW>");
		}

		protected virtual void _(Events.FieldDefaulting<SOInvoice, SOInvoice.refNbr> e)
		{
			var row = e.Row;
			e.Cache.SetValue(row, "RefNbr", "<NEW>");
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice, SOInvoice.refNbr> e)
		{
			var row = e.Row;
			e.Cache.SetValue(row, 0, "<NEW>");
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