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
		protected virtual void SOInvoice_RefNbr_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			
		}

		protected virtual void SOInvoice_RefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{

		}

		protected virtual void SOInvoice_RefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{

		}

		protected virtual void SOInvoice_RefNbr_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{

		}

		protected virtual void SOInvoice_RefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{

		}

		protected virtual void SOInvoice_RefNbr_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
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