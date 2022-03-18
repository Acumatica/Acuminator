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
		public PXSelect<SOInvoice> Invoices;

		public int Mode { get; set; }

		protected virtual void _(Events.RowPersisted<SOInvoice> e)
		{
			switch (Mode)
			{
				case 0:
					throw new PXRowPersistedException(typeof(SOInvoice.refNbr).Name, e.Row.RefNbr, "Persist error");			   //No diagnostic
				case 1:
					throw new PXLockViolationException(typeof(SOInvoice), PXDBOperation.Insert, new object[] { e.Row.RefNbr });    //No diagnostic
				default:
					throw new PXException("Something bad happened");        //Should report diagnostic
			}	
		}

		protected virtual void SOLine_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			SOLine invoiceLine = e.Row as SOLine;

			switch (Mode)
			{
				case 0:
					throw new PXRowPersistedException(typeof(SOLine.refNbr).Name, invoiceLine.RefNbr, "Persist error");									  //No diagnostic
				case 1:
					throw new PXLockViolationException(typeof(SOLine), PXDBOperation.Insert, new object[] { invoiceLine.RefNbr, invoiceLine.LineNbr });   //No diagnostic
				default:
					throw new PXException("Something bad happened");        //Should report diagnostic
			}
		}
	}

	public class SOInvoice : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }

		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion

		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

		[PXDBInt]
		[PXDefault(0)]
		public virtual int? LineCntr
		{
			get;
			set;
		}
		#endregion
	}

	public class SOLine : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }

		public abstract class refNbr : IBqlField { }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOInvoice.lineCntr))]
		public int? LineNbr { get; set; }

		public abstract class lineNbr : IBqlField { }
		#endregion
	}

	public class ARInvoice : IBqlTable
	{
	}
}