using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOSomeAttribute : PXEventSubscriberAttribute, 
								   IPXRowDeletingSubscriber, IPXRowDeletedSubscriber, 
								   IPXRowInsertingSubscriber, IPXRowInsertedSubscriber,
								   IPXRowUpdatingSubscriber, IPXRowUpdatedSubscriber,
								   IPXRowSelectingSubscriber, IPXRowSelectedSubscriber,
								   IPXRowPersistingSubscriber, 
								   IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber
	{
		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			throw new PXException("Something bad happened");
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			throw new PXException("Something bad happened");
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			throw new PXException("Something bad happened");
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			throw new PXException("Something bad happened");
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			throw new PXException("Something bad happened");
		}

		public void RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			throw new PXException("Something bad happened");
		}

		public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			throw new NotSupportedException();
		}

		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			throw new ArgumentException("Something bad happened");
		}

		public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			throw new Exception("Something bad happened");
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			throw new ArgumentNullException();
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			throw new InvalidOperationException("Something bad happened");
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