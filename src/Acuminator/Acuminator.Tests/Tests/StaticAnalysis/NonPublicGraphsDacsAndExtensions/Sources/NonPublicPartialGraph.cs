using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicGraphsDacsAndExtensions.Sources
{
	static partial class PurchaseGlobalState
	{
		partial class SOOrderPurchaseEntry : PXGraph<SOOrderPurchaseEntry>
		{
			public void _(Events.RowSelected<SOOrder> e)
			{

			}
		}

		private partial class SOOrderPurchaseEntry2 : PXGraph<SOOrderPurchaseEntry2>
		{
			public void _(Events.RowSelected<SOOrder> e)
			{

			}
		}
	}

	static partial class PurchaseGlobalState
	{
		partial class SOOrderPurchaseEntry2 : PXGraph<SOOrderPurchaseEntry2>
		{
		}
	}

	internal static partial class PurchaseGlobalState
	{
		public partial class SOOrderPurchaseEntry : PXGraph<SOOrderPurchaseEntry>
		{
		}
	}


	[PXCacheName("SO Order")]
	public class SOOrder : IBqlTable
	{

	}
}
