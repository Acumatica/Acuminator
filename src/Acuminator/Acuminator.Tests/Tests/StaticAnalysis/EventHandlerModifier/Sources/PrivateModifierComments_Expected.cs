using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
	}

	public sealed class MyExtension : PXGraphExtension<MyGraph>
	{
		// First line
		// Second line
		public void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
			return;
		}
	}
}