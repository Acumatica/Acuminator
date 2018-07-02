using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
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

		#region TestField
		public abstract class Tstamp : IBqlField
		{
		}

		private readonly byte[] test_field;

		[PXDBTimestamp]
		public virtual byte[] TestProperty
		{
			get;
			set;
		}
		#endregion
	}
}