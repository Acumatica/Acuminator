using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public abstract class MyExtension : PXGraphExtension<MyGraph>
	{
		protected abstract void HandleCacheAttachedDiscDate(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e);
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}