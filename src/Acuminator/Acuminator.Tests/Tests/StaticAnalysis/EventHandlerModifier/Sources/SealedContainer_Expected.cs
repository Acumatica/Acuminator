using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public sealed class MyExtension : PXGraph<MyGraph>
	{
		public void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
			return;
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}