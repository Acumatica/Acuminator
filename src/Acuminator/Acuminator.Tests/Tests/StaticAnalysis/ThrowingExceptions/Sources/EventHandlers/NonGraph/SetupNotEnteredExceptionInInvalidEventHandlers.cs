using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOSomeAttribute : PXEventSubscriberAttribute, 
								   IPXRowSelectingSubscriber, 
								   IPXRowUpdatingSubscriber, IPXRowUpdatedSubscriber,
								   IPXRowInsertingSubscriber, IPXRowInsertedSubscriber, 
								   IPXRowDeletingSubscriber, IPXRowDeletedSubscriber,
								   IPXRowPersistingSubscriber, IPXRowPersistedSubscriber,
								   IPXFieldUpdatingSubscriber, IPXFieldUpdatedSubscriber,
								   IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber, IPXFieldSelectingSubscriber,
								   IPXExceptionHandlingSubscriber, IPXCommandPreparingSubscriber
	{
		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
		}

		public void ExceptionHandling(PXCache sender, PXExceptionHandlingEventArgs e) =>
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e) => 
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e) => 
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e) => 
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) => 
			throw new PXSetupNotEnteredException("Setup is not entered", typeof(SOInvoice));
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