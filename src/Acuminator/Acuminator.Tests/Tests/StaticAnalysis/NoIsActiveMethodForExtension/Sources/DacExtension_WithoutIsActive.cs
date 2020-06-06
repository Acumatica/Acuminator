using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicExtensions.Sources
{
	public sealed class SOOrderExt : PXCacheExtension<SOOrder>
	{
	}

	public sealed class SOOrderExtDiscount : PXCacheExtension<SOOrder>
	{
		public static void IsActive() { }

		public static bool IsActive<T>() => false;
	}

	public sealed class SOOrderExt1 : PXCacheExtension<SOOrder>
	{
		public void IsActive() { }
	}

	public sealed class SOOrderExt2 : PXCacheExtension<SOOrder>
	{
		public static int IsActive() => 0;

		public static bool IsActive(object obj) => true;
	}

	public sealed class SOOrderExtSales1 : PXCacheExtension<SOOrder>
	{
		public static bool IsActive => true;  //Property is not supported
	}



	[PXHidden]
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
		[PXString]
		public string Status { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField
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
}
