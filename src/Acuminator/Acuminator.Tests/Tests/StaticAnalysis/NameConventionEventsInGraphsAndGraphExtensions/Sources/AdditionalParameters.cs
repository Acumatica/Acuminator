using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry>
	{
		public SOInvoiceEntry()
		{
			this.RowSelected.AddHandler<SOInvoice>((s, e) => OnRowSelected(s, e, true));
		}

		protected override void OnRowSelected(PXCache cache, PXRowSelectedEventArgs e, bool flag)
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