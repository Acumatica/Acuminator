using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicExtensions.Sources
{
	internal class SOOrderExt1 : PXGraphExtension<SOOrderEntry>
	{
		public virtual void _(Events.RowUpdating<SOOrder> e)
		{

		}
	}

	class SOOrderExt2 : PXGraphExtension<SOOrderEntry>
	{
		public virtual void _(Events.RowUpdated<SOOrder> e)
		{

		}
	}

	static class PurchaseGlobalState
	{
		private class PurchaseEngine
		{
			public sealed class SOOrderEntryExtPurchase : PXGraphExtension<SOOrderEntry>
			{
				public virtual void _(Events.RowSelected<SOOrder> e)
				{

				}
			}

			protected internal sealed class SOOrderEntryExtPurchase2 : PXGraphExtension<SOOrderEntry>
			{
				public virtual void _(Events.RowSelected<SOOrder> e)
				{

				}
			}
		}
	}


	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{

	}

	[PXHidden]
	public class SOOrder : IBqlTable
	{

	}
}
