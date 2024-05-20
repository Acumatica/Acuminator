using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicGraphsDacsAndExtensions.Sources
{
	public static partial class PurchaseGlobalState
	{
		public partial class SOOrderPurchaseEntry : PXGraph<SOOrderPurchaseEntry>
		{
			public void _(Events.RowSelected<SOOrder> e)
			{

			}
		}

		public partial class SOOrderPurchaseEntry2 : PXGraph<SOOrderPurchaseEntry2>
		{
			public void _(Events.RowSelected<SOOrder> e)
			{

			}
		}
	}

	public static partial class PurchaseGlobalState
	{
		public partial class SOOrderPurchaseEntry2 : PXGraph<SOOrderPurchaseEntry2>
		{

		}
	}

	public static partial class PurchaseGlobalState
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
