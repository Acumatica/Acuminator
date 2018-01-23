using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects
{
	public class SOOrder : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : IBqlField { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }
		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
	}

	public class Open : Constant<string>
	{
		public Open() : base("O")
		{
		}
	}

	public class SalesOrder : Constant<string>
	{
		public SalesOrder() : base("SO")
		{
		}
	}

	public class BAccount : IBqlTable
	{
		#region BAccountID
		[PXDBIdentity]
		public int? BAccountID { get; set; }
		public abstract class bAccountID : IBqlField { }
		#endregion

		#region AcctCD
		[PXDBString]
		public string AcctCD { get; set; }
		public abstract class acctCD : IBqlField { }
		#endregion

		#region AcctName
		[PXDBString]
		public string AcctName { get; set; }
		public abstract class acctName : IBqlField { }
		#endregion
	}
}
