using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyExtension : ParentExtension
	{
		protected void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e) // compiler doesn't identify polymorphism, wrong modifier will be suggested.
		{
			return;
		}

		private void HandleFieldUpdatedDiscDate(Events.FieldUpdated<PX.Objects.AR.ARInvoice.discDate> e)
		{
			return;
		}

		public void HandleCacheAttachedCuryDocBal(Events.CacheAttached<PX.Objects.AR.ARInvoice.curyDocBal> e)
		{
			return;
		}

		private protected void HandleCacheAttachedDocType(Events.CacheAttached<PX.Objects.AR.ARInvoice.docType> e)
		{
			return;
		}

		protected internal void HandleCacheAttachedRefNbr(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
			return;
		}

		protected sealed override void HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
			return;
		}
	}

	public class ParentExtension : PXGraph<MyGraph>
	{
		protected virtual void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
			return;
		}

		protected virtual void HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
			return;
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}