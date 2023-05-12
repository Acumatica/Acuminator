using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public abstract class WarehouseManagementSystem<TSelf, TGraph> : PXGraphExtension<TGraph>
	{
		public abstract class ScanExtension : PXGraphExtension<TSelf, TGraph>
		{
			public virtual object TestMethod(int x, bool drilldown)
			{
				return new object();
			}
		}
	}

	public class PickPackShip : WarehouseManagementSystem<PickPackShip, PickPackShip.Host>
	{

		public class Host : PXGraph<Host>
		{
		}
	}

	public class PickPackShipExt : PickPackShip.ScanExtension
	{
		[PXOverride]
		public virtual object TestMethod(int x, bool drilldown)
		{
			return new object();
		}
	}
}