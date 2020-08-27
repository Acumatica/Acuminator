using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.Sources
{
	[PXCacheName("SO Order")]
	public class SOOrderNoPKOneKey : IBqlTable
	{
		public class PK : PrimaryKeyOf<SOOrderNoPKOneKey>.By<orderType, orderNbr>
		{
			public static SOOrderNoPKOneKey Find(PXGraph graph, string orderType, string orderNbr) => FindBy(graph, orderType, orderNbr);
		}

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		public abstract class orderNbr : IBqlField { }

		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXDBString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		public abstract class status : IBqlField { }

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		public abstract class Tstamp : IBqlField { }
	}
}
