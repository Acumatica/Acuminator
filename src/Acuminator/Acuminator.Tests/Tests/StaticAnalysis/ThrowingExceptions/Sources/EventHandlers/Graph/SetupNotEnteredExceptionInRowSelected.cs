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
			throw new PXSetupNotEnteredException("Setup is not entered");
		}

		protected virtual void _(Events.RowSelected<ARInvoice> e)
		{
			var ex = new PXSetupNotEnteredException("Setup is not entered");
			throw ex;
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

	public class ARInvoice : IBqlTable
	{
	}
}