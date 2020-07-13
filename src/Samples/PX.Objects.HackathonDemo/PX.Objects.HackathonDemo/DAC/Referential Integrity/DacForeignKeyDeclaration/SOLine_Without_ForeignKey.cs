using System;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.HackathonDemo.ReferentialIntegrity
{
	[PXCacheName("SO Line")]
	public partial class SOLine : PX.Data.IBqlTable
	{
		public class PK : PrimaryKeyOf<SOLine>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLine Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(typeof(SOOrder.branchID))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SOOrder.orderType))]
		[PXUIField(DisplayName = "Order Type", Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<SOOrderType.orderType>), CacheGlobal = true)]
		public virtual string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOOrder.orderNbr), DefaultForUpdate = false)]
		[PXParent(typeof(Select<SOOrder,
							Where<SOOrder.orderType, Equal<Current<SOLine.orderType>>, 
							  And<SOOrder.orderNbr, Equal<Current<SOLine.orderNbr>>>>>))]
		[PXUIField(DisplayName = "Order Nbr.", Visible = false, Enabled = false)]
		public virtual string OrderNbr { get; set; }
		#endregion

		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public virtual int? LineNbr { get; set; }
		#endregion
	}
}
