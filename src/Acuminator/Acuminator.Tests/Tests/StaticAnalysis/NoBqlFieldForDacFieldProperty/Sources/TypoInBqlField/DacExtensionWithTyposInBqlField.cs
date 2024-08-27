using System;

using PX.Data;
using PX.Data.BQL;

namespace PX.Analyzers.Test.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXHidden]
	public sealed class DacExtension : PXCacheExtension<BaseDac>
	{
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }

		#region Selected
		public abstract class seleced : BqlBool.Field<seleced> { }

		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		}
		#endregion
	}

	[PXHidden]
	public class BaseDac : IBqlTable
	{
		#region Status
		public abstract class stattus : PX.Data.BQL.BqlString.Field<stattus> { }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : BqlString.Field<orderNbr> { }

		[PXDBString(4, IsUnicode = true)]
		[PXUIField(DisplayName = "OrderNbr")]
		public virtual string OrderNbr
		{
			get;
			set;
		}
		#endregion

		public virtual string ExtraData { get; set; }
	}
}