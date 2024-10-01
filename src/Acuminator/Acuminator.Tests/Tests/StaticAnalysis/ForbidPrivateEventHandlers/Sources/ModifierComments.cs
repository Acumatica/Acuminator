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
		private void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		// comments
		void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}
	}

	public abstract class MyAbstractExtension : PXGraphExtension<MyGraph>
	{
		// First line
		// Second line
		public abstract void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e);

		// comments
		abstract public void HandleCacheAttachedApproverID(Events.CacheAttached<PX.Objects.AR.ARInvoice.approverID> e);

		// comments
		abstract void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e);

		// comments
		public abstract /* new */ void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.docType> e);

		// comments
		abstract /* new */ public /* further comments */ void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.curyDocBal> e);
	}

	public class MyExtension : PXGraphExtension<MyGraph>
	{
		// First line
		// Second line
		private void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}

		// comments
		void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.refNbr> e)
		{
		}

		/// <summary>
		/// This is an event handler
		/// </summary>
		/// <param name="e">Event handler argument</param>
		private void HandleCacheAttachedApproverID(Events.CacheAttached<PX.Objects.AR.ARInvoice.approverID> e)
		{
		}
	}
}