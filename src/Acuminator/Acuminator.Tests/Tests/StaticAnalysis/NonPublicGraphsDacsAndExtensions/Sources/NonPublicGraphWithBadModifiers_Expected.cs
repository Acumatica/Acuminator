using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicGraphsDacsAndExtensions.Sources
{
	public static class PurchaseGlobalState
	{
		public class PurchaseEngine
		{
			public sealed class SOOrderPurchaseEntry : PXGraph<SOOrderPurchaseEntry>
			{
				public void _(Events.RowSelected<SOOrder> e)
				{

				}
			}

			public sealed class SOOrderPurchaseEntry2 : PXGraph<SOOrderPurchaseEntry2>
			{
				public void _(Events.RowSelected<SOOrder> e)
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
