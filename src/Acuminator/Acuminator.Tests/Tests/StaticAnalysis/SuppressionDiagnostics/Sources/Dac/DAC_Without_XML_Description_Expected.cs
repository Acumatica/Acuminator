using System;
using PX.Data;

namespace PX.Objects
{
	/// <summary>
	/// Some description here to avoid displaying diagnostic on the DAC itself in order to test how exclude code fix application to DAC properties.
	/// </summary>
	[PXCacheName("DAC")]
	public class DAC : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : IBqlField { }

		///
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later
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
		// Acuminator disable once PX1007 NoXmlCommentForPublicEntityOrDacProperty to be documented later
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
	}
}
