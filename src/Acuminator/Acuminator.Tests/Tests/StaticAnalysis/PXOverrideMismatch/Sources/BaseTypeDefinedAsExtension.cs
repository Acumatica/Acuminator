using System;

namespace Acuminator.Tests.Sources
{
	public abstract class WarehouseManagementSystem<TSelf, TGraph> : PX.Data.PXGraphExtension<TGraph>
	{
		public abstract class ScanExtension : PX.Data.PXGraphExtension<TSelf, TGraph>
		{
			public object TestMethod(int x, bool drilldown)
			{
				return new object();
			}
		}
	}

	public class PickPackShip : WarehouseManagementSystem<PickPackShip, PickPackShip.Host>
	{

		public class Host
		{
		}
	}

	public class PickPackShipExt : PickPackShip.ScanExtension
	{
		[PX.Data.PXOverride]
		public virtual object TestMethod(int x, bool drilldown)
		{
			return new object();
		}
	}
}

namespace PX.Data
{
	public class PXOverrideAttribute : Attribute
	{
	}

	public abstract class PXGraphExtension<Graph>
	{
		internal Graph _Base;
	}

	public abstract class PXGraphExtension<Extension1, Graph>
	{
		internal Extension1 MyExt;

		internal Graph MyGraph;
	}
}