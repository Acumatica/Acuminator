using System;
using PX.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects
{
	[PXHidden]
	public class DAC1 : IBqlTable
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

		///
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		/// <summary>
		///
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
	}


	/// <summary>
	/// A DAC 2.
	/// </summary>
	[PXCacheName("DAC 2")]
	public class DAC2 : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		[Obsolete]
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion
	}


	[Obsolete]
	public class DAC3 : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion
	}



	[PX.Common.PXInternalUseOnly]
	public class DAC4 : IBqlTable
	{
		#region Status
		public abstract class status : IBqlField { }

		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
	}
}
