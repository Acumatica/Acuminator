using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyExtension : ParentExtension
	{
		protected virtual new void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		protected virtual void HandleFieldUpdatedDiscDate(Events.FieldUpdated<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		protected virtual void HandleCacheAttachedCuryDocBal(Events.CacheAttached<PX.Objects.AR.ARInvoice.curyDocBal> e)
		{
		}

		protected virtual void HandleCacheAttachedDocType(Events.CacheAttached<PX.Objects.AR.ARInvoice.docType> e)
		{
		}

		protected virtual void HandleCacheAttachedRefNbr(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}

		protected sealed override void HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
		}

		protected virtual void HandleCacheAttachedApproverID(Events.CacheAttached<PX.Objects.AR.ARInvoice.approverID> e)
		{
		}
	}

	public class ParentExtension : PXGraph<MyGraph>
	{
		protected virtual void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
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