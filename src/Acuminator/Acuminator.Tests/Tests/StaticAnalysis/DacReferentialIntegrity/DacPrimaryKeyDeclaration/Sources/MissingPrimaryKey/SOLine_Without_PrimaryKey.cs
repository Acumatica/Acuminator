using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.Sources
{
	[PXCacheName("SO Line")]
	public class SOLine : IBqlTable
	{
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type", Visible = false, Enabled = false)]
		public virtual string OrderType
		{
			get;
			set;
		}

		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.", Visible = false, Enabled = false)]
		public virtual string OrderNbr
		{
			get;
			set;
		}

		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual int? LineNbr
		{
			get;
			set;
		}
	}
}