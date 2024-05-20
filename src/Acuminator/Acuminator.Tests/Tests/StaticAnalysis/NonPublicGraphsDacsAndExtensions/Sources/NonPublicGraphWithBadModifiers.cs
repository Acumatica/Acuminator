using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicGraphsDacsAndExtensions.Sources
{
	static class PurchaseGlobalState
	{
		private class PurchaseEngine
		{
			private sealed protected class SOOrderPurchaseEntry : PXGraph<SOOrderPurchaseEntry>
			{
				public void _(Events.RowSelected<SOOrder> e)
				{

				}
			}

			sealed internal protected class SOOrderPurchaseEntry2 : PXGraph<SOOrderPurchaseEntry2>
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
