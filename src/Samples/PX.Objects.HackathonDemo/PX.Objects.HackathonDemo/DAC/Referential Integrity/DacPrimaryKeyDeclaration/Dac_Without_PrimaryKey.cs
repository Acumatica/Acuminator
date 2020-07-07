using PX.Data;

namespace PX.Objects.HackathonDemo.ReferentialIntegrity.NoPrimaryKey
{
	[PXPrimaryGraph(typeof(SO.SOOrderEntry))]
	public class SOOrder : IBqlTable
	{
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

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Extra key")]
		public string ExtraKey { get; set; }

		public abstract class extraKey : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Another Extra Key")]
		public string AnotherExtraKey { get; set; }

		public abstract class anotherExtraKey : IBqlField { }

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
