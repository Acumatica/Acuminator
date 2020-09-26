using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.DacForeignKeyDeclaration.Sources.UnboundFieldInKey
{
	[PXCacheName("SO Line")]
	public partial class SOLineWithUnboundFieldInCompositeFK : IBqlTable
	{
		public class PK : PrimaryKeyOf<SOLineWithUnboundFieldInCompositeFK>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLineWithUnboundFieldInCompositeFK Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}

		public static class FK
		{
			public class SOOrderCompositeFK : CompositeKey<
														  Field<orderNbr>.IsRelatedTo<SOOrder.orderNbr>,
														  Field<orderType>.IsRelatedTo<SOOrder.orderType>											  
														 >
														 .WithTablesOf<SOOrder, SOLineWithUnboundFieldInCompositeFK>
			{ }
		}

		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SOOrder.orderType))]
		[PXUIField(DisplayName = "Order Type", Visible = false, Enabled = false)]
		public virtual string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		
		[PXString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOOrder.orderNbr), DefaultForUpdate = false)]
		[PXParent(typeof(Select<SOOrder,
							Where<SOOrder.orderType, Equal<Current<orderType>>,
							  And<SOOrder.orderNbr, Equal<Current<orderNbr>>>>>))]
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
