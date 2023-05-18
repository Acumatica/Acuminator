using System;
using PX.Data;

namespace PX.Objects
{
	/// <inheritdoc/>
	[PXCacheName("NonProjection DAC")]
	public class NonProjectionDacInhertitdoc : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		/// <inheritdoc/>
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : IBqlField { }

		/// <inheritdoc cref="POOrder.OrderNbr"/>
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		/// <inheritdoc cref="POOrder.Status" path="/summary"/>
		/// <value>
		/// The status
		/// </value>
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
	}

	[PXHidden]
	public class POOrder : IBqlTable
	{
		#region OrderNbr
		public abstract class orderNbr : IBqlField { }

		/// <summary>
		/// Gets or sets the order number.
		/// </summary>
		/// <value>
		/// The order number.
		/// </value>
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		/// <summary>
		/// PO Order
		/// </summary>
		/// <remarks>
		/// Test
		/// </remarks>
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
	}
}
