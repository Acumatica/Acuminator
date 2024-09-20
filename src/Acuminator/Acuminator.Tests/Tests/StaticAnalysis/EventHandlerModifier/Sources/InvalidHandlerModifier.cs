using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyExtension : ParentExtension
	{
		protected new void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		private void HandleFieldUpdatedDiscDate(Events.FieldUpdated<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		public void HandleCacheAttachedCuryDocBal(Events.CacheAttached<PX.Objects.AR.ARInvoice.curyDocBal> e)
		{
		}

		private protected void HandleCacheAttachedDocType(Events.CacheAttached<PX.Objects.AR.ARInvoice.docType> e)
		{
		}

		protected internal void HandleCacheAttachedRefNbr(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}

		protected sealed override void HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
		}

		internal virtual protected void HandleCacheAttachedApproverID(Events.CacheAttached<PX.Objects.AR.ARInvoice.approverID> e)
		{
		}
	}

	public class ParentExtension : PXGraph<MyGraph>
	{
		protected void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		protected virtual void HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}