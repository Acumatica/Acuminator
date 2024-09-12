using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		private void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.curyDocBal> e)
		{
			return;
		}

		public virtual void _(Events.FieldUpdated<PX.Objects.AR.ARInvoice.docType> e)
		{
			return;
		}
	}

	public class MyExtension : PXGraphExtension<MyGraph>
	{
		private void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
			return;
		}

		protected void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.docType> e)
		{
			return;
		}

		protected internal void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
			return;
		}

		private protected void _(Events.CacheAttached<PX.Objects.CA.CABatch.batchSeqNbr> e)
		{
			return;
		}

		public sealed override void _(Events.FieldUpdated<PX.Objects.AR.ARInvoice.docType> e)
		{
			return;
		}

		protected virtual void _(Events.FieldUpdated<PX.Objects.AR.ARInvoice.refNbr> e)
		{
			return;
		}
	}
}