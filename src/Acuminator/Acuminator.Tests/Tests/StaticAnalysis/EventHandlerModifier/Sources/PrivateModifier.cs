using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		private void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.curyDocBal> e)
		{
		}

		public virtual void _(Events.FieldUpdated<PX.Objects.AR.ARInvoice.docType> e)
		{
		}
	}

	public class MyExtension : PXGraphExtension<MyGraph>
	{
		private void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		protected void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.docType> e)
		{
		}

		protected internal void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}

		private protected void _(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
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