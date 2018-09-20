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
		public PXSelect<SOInvoice> Documents;

		protected virtual void _(Events.RowInserting<SOInvoice> e)
		{
			e.Row?.RefNbr = "<NEW>"; // OK
			Documents.Current.RefNbr = "<NEW>"; // not OK
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