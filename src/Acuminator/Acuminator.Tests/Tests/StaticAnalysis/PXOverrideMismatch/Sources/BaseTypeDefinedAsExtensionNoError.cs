using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public abstract class WarehouseManagementSystem<TSelf, TGraph> : PXGraphExtension<TGraph> where TSelf : PXGraphExtension<TGraph> where TGraph : PXGraph
	{
		public abstract class ScanExtension : PXGraphExtension<TSelf, TGraph>
		{
		}
	}

	public class PickPackShip : WarehouseManagementSystem<PickPackShip, PickPackShip.Host>
	{
		public class Host : PXGraph<Host>
		{
			public virtual object TestMethod(int x, bool drilldown)
			{
				return new object();
			}
		}
	}

	public class PickPackShipExt : PickPackShip.ScanExtension
	{
		[PXOverride]
		public object TestMethod(int x, bool drilldown)
		{
			return new object();
		}
	}
}