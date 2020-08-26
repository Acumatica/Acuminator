using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.HackathonDemo.ReferentialIntegrity.MultipleUniqueKeys
{
	[PXCacheName("SO Order")]
	public class SOOrderMultipleUniqueKeysWrongDeclaration : IBqlTable
	{
		public class PK : PrimaryKeyOf<SOOrder>.By<orderType, orderNbr>
		{
			public static SOOrder Find(PXGraph graph, string orderType, string orderNbr) => FindBy(graph, orderType, orderNbr);
		}

		public class UniqueKey1 : PrimaryKeyOf<SOOrder>.By<orderType>
		{
			public static SOOrder Find(PXGraph graph, string orderType) => FindBy(graph, orderType);
		}

		public class UniqueKey2 : PrimaryKeyOf<SOOrder>.By<orderType, orderNbr, status>
		{
			public static SOOrder Find(PXGraph graph, string orderType, string orderNbr, string status) => FindBy(graph, orderType, orderNbr, status);
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
