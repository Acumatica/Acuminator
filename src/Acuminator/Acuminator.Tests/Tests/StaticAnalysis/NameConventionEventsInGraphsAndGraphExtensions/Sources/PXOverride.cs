using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
	{
		[PXOverride]
		public virtual void SOInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			
		}
	}

	public class SOInvoiceEntry : SOInvoiceEntryBase
	{
		protected virtual void SOInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
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