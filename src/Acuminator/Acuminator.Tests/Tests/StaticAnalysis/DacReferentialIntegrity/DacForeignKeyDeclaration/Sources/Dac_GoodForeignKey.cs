using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.Sources
{
	[PXCacheName("SO Order")]
	public partial class SOLine : PX.Data.IBqlTable
	{
		public class PK : PrimaryKeyOf<SOLine>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLine Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}

		public static class FK
		{
			public class SOOrder : PX.Objects.SO.SOOrder.PK.ForeignKeyOf<SOLine>.By<orderType, orderNbr> { }
		}

		public abstract class orderType : IBqlField { }

		public abstract class orderNbr : IBqlField { }

		public abstract class lineNbr : IBqlField { }
	}


	[PXCacheName("SO Line")]
	public class SOOrder : IBqlTable
	{
		public class PK : PrimaryKeyOf<SOLine>.By<orderType, orderNbr>
		{
			public static SOLine Find(PXGraph graph, string orderType, string orderNbr) => FindBy(graph, orderType, orderNbr);
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
