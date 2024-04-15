using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicGraphsDacsAndExtensions.Sources
{
	public class SOOrderGraph : PXGraph<SOOrderGraph>
	{
		public virtual void _(Events.RowUpdating<SOOrder> e)
		{

		}
	}

	public class SOOrderGraph2 : PXGraph<SOOrderGraph2>
	{
		public virtual void _(Events.RowUpdated<SOOrder> e)
		{

		}
	}

	public static class PurchaseGlobalState
	{
		public class PurchaseEngine
		{
			public sealed class SOOrderPurchaseEntry : PXGraph<SOOrderPurchaseEntry>
			{
				public virtual void _(Events.RowSelected<SOOrder> e)
				{

				}
			}

			public sealed class SOOrderPurchaseEntry2 : PXGraph<SOOrderPurchaseEntry2>
			{
				public virtual void _(Events.RowSelected<SOOrder> e)
				{

				}
			}
		}
	}

	[PXCacheName("SO Order")]
	public class SOOrder : IBqlTable
	{

	}
}
