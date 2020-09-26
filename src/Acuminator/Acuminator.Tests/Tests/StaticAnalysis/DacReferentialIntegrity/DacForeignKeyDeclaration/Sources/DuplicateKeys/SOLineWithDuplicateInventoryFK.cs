using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.DacForeignKeyDeclaration.Sources.DuplicateKeys
{
	[PXCacheName("SO Line")]
	public partial class SOLineWithDuplicateInventoryFK : IBqlTable
	{
		public class PK : PrimaryKeyOf<SOLineWithDuplicateInventoryFK>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLineWithDuplicateInventoryFK Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}

		public static class FK
		{
			public class InventorySimpleFK : Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>.AsSimpleKey.WithTablesOf<InventoryItem, SOLineWithDuplicateInventoryFK> { }

			public class InventoryFkViaPK : InventoryItem.PK.ForeignKeyOf<SOLineWithDuplicateInventoryFK>.By<inventoryID> { }
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

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
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

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[PX.Objects.SO.SOLineInventoryItem(Filterable = true)]
		[PXDefault]
		public virtual int? InventoryID { get; set; }
		#endregion
	}
}
