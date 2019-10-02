using PX.Data;
using System;

namespace Acuminator.Tests.Sources
{
	[PXHidden]
	public sealed class SOShipmentExt : PXCacheExtension<SOOrderMaintSync.NestedSOOrder>
	{
		public abstract class shipmentNbr : IBqlField { }

		[PXInt]
		public int? ShipmentNbr
		{
			get;
			set;
		}

		public abstract class shipmentNbr2 : IBqlField { }

		[PXInt]
		public int? ShipmentNbr2
		{
			get;
			set;
		}
	}

	public class SOOrderMaintSync : PXGraph<SOOrderMaintSync>
	{
		[PXHidden]
		public class NestedSOOrder : IBqlTable
		{
			public abstract class orderNbr : IBqlField { }

			[PXDBString(length: 30, IsKey = true)]
			public virtual string OrderNbr
			{
				get;
				set;
			}
		}
	}
}
