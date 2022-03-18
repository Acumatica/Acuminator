using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOSomeAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
	{
		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var ex = new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
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