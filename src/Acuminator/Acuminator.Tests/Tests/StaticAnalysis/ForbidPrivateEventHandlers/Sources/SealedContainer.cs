﻿using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public sealed class MyExtension : PXGraph<MyGraph>
	{
		private void _(Events.CacheAttached<PX.Objects.AR.ARInvoice.discDate> e)
		{
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}
}