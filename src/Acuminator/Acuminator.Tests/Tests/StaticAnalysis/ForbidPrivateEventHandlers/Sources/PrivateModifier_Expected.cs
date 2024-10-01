using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		protected virtual void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.curyDocBal> e)
		{
		}

		protected virtual void _(Events.FieldUpdated<PX.Objects.AR.ARInvoice.docType> e)
		{
		}
	}

	public class MyExtension : PXGraphExtension<MyGraph>
	{
		protected virtual void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		protected virtual void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.docType> e)
		{
		}

		protected virtual void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}

		protected virtual void _(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
		}

		public sealed override void _(Events.FieldUpdated<PX.Objects.AR.ARInvoice.docType> e)
		{
		}

		protected virtual void _(Events.FieldUpdated<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}
	}
}