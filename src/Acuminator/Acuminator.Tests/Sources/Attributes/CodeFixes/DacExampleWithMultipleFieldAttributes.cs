using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class DacExampleWithMultipleFieldAttributes : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		[PXDBString]
		[PXDBInt]
		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXUIField(DisplayName = "Status"), PXDate, PXInt]
		[PXDBField]
		public string Status { get; set; }
		#endregion
	}
}
