using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SO_Order : IBqlTable
	{
		#region Order_Type
		public abstract class order_Type : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string Order_Type { get; set; }
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
		
		#region OrderDate
		public abstract class __ : IBqlField { }

		[PXDBInt]
		[PXUIField(DisplayName = "OrderDate")]
		public DateTime? __ { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField
		{
		}

		private byte[] tstamp_field;

		[PXDBTimestamp]
		public virtual byte[] tstamp_property
		{
			get { return tstamp_field; }
			set { tstamp_field = value; }
		}
		#endregion
	}
}
