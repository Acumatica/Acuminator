using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	[PXHidden]
	public class SOOrder : IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXUIField(DisplayName = "Status")]
		[PXDBString]
		public string Status { get; set; }
		#endregion

		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }

		[PXDBDate]
		[PXUIField(DisplayName = "OrderDate")]
		public DateTime? OrderDate { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}

	public sealed class SOOrderWithTotal : PXCacheExtension<SOOrder>
	{
		#region Total
		public abstract class total : PX.Data.BQL.BqlDecimal.Field<total> { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal? Total { get; set; }
		#endregion
	}

	public sealed class SOOrderWithHold : PXCacheExtension<SOOrder>
	{
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Hold")]
		public bool? Hold { get; set; }
		#endregion
	}
}