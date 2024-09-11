using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyExtension : PXGraph<MyGraph>, IExtension
	{
		public void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
			return;
		}

		public void HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
			return;
		}
	}

	public interface IExtension
	{
		void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e);

		void HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e);
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}