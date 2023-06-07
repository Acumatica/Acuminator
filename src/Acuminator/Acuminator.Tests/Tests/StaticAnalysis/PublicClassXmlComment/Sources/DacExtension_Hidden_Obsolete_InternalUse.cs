using System;
using PX.Data;
using System.Collections.Generic;

namespace PX.Objects
{
	[PXHidden]
	public class Dac : IBqlTable
	{
	}

	[PXHidden]
	public sealed class HiddenDacExtension : PXCacheExtension<Dac>  // Should be reported
	{
		#region Status
		public abstract class status : IBqlField { }

		/// <summary>
		///
		/// </summary>
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }          // Should be reported
		#endregion
	}


	/// <summary>
	/// Obsolete extension.
	/// </summary>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXCacheName("DAC 2")]
	public sealed class ObsoleteExtension : PXCacheExtension<Dac>
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		[Obsolete]
		[PXDBString(IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion
	}


	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[Obsolete]
	public sealed class ObsoleteExtension2 : PXCacheExtension<Dac>
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion
	}



	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PX.Common.PXInternalUseOnly]
	public sealed class InternalExtension : PXCacheExtension<Dac>
	{
		#region Status
		public abstract class status : IBqlField { }

		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion
	}
}
