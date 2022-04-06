using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOSomeAttribute : PXEventSubscriberAttribute, IPXRowPersistedSubscriber
	{
		public int Mode { get; set; }

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			SOInvoice invoice = e.Row as SOInvoice;

			switch (Mode)
			{
				case 0:
					throw new PXRowPersistedException(typeof(SOInvoice.refNbr).Name, invoice.RefNbr, "Persist error");              //No diagnostic
				case 1:
					throw new PXLockViolationException(typeof(SOInvoice), PXDBOperation.Insert, new object[] { invoice.RefNbr });   //No diagnostic
				case 2:
					throw new PXException("Something bad happened");        //Should report diagnostic
				case 3:
					throw new ArgumentOutOfRangeException(nameof(Mode));    //No diagnostic
				case 4:
					throw new ArgumentNullException(nameof(Mode));          //No diagnostic
				case 5:
					throw new ArgumentException("Something bad happened");  //No diagnostic
				case 6:
					throw new NotImplementedException();                    //No diagnostic
				default:
					throw new NotSupportedException();                      //No diagnostic
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
	}

	public class ARInvoice : IBqlTable
	{
	}
}