﻿using System;
using PX.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		/// <summary>
		/// Order type
		/// </summary>
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : IBqlField { }

		/// <summary>
		/// Order nbr
		/// </summary>
		///
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		/// <summary>
		/// Status
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
	}
}
