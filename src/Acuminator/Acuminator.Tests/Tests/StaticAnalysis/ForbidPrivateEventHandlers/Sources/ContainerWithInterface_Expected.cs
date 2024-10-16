﻿using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyExtension : PXGraph<MyGraph>, IExtension
	{
		public void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		public void IExtension.HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
		}

		// comments: we don't propose a fix for this case, we just produce a warning.
		public void IExtension.HandleCacheAttachedRefNbr(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}
	}

	public interface IExtension
	{
		void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e);

		void HandleCacheAttachedBatchSeqNbr(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e);

		void HandleCacheAttachedRefNbr(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e);
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}