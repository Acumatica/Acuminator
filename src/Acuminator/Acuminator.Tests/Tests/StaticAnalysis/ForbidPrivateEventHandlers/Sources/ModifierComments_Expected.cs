using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
	}

	public sealed class MySealedExtension : PXGraphExtension<MyGraph>
	{
		// First line
		// Second line
		public void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		// comments
		public void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}
	}

	public abstract class MyAbstractExtension : PXGraphExtension<MyGraph>
	{
		// First line
		// Second line
		protected abstract void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e);

		// comments
		protected abstract void HandleCacheAttachedApproverID(Events.CacheAttached<PX.Objects.AR.ARInvoice.approverID> e);

		// comments
		protected abstract void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e);

		// comments
		protected abstract /* new */ void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.docType> e);

		// comments
		protected /* further comments */ abstract /* new */ void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.curyDocBal> e);
	}

	public class MyExtension : PXGraphExtension<MyGraph>
	{
		// First line
		// Second line
		protected virtual void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		// comments
		protected virtual void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}

		/// <summary>
		/// This is an event handler
		/// </summary>
		/// <param name="e">Event handler argument</param>
		protected virtual void HandleCacheAttachedApproverID(Events.CacheAttached<PX.Objects.AR.ARInvoice.approverID> e)
		{
		}
	}
}