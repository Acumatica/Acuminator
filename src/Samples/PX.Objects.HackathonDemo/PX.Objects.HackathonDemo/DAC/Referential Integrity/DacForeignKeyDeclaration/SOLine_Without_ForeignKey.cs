using System;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

using GLBranchAttribute = PX.Objects.GL.BranchAttribute;


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

		[GLBranch(typeof(SOOrder.branchID))]
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

		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }

		[PXDBDate]
		[PXDBDefault(typeof(SOOrder.orderDate))]
		public virtual DateTime? OrderDate { get; set; }
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[SOLineInventoryItem(Filterable = true)]
		[PXDefault]
		[PXForeignReference(typeof(IN.InventoryItem.PK.ForeignKeyOf<SOLine>.By<inventoryID>))]
		public virtual int? InventoryID { get; set; }
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<SOLine.inventoryID>))]
		[SubItem(typeof(SOLine.inventoryID))]
		[SubItemStatusVeryfier(typeof(SOLine.inventoryID), typeof(SOLine.siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
		public virtual int? SubItemID { get; set; }
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		[SiteAvail(typeof(SOLine.inventoryID), typeof(SOLine.subItemID))]
		[PXParent(typeof(Select<SOOrderSite, 
							Where<SOOrderSite.orderType, Equal<Current<SOLine.orderType>>, 
								And<SOOrderSite.orderNbr, Equal<Current<SOLine.orderNbr>>, 
								And<SOOrderSite.siteID, Equal<Current2<SOLine.siteID>>>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? SiteID { get; set; }
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[AP.Vendor(typeof(Search<BAccountR.bAccountID,
			Where<AP.Vendor.type, NotEqual<BAccountType.employeeType>>>))]
		[PXRestrictor(typeof(Where<AP.Vendor.status, IsNull,
								Or<AP.Vendor.status, Equal<BAccount.status.active>,
								Or<AP.Vendor.status, Equal<BAccount.status.oneTime>>>>), AP.Messages.VendorIsInStatus, typeof(AP.Vendor.status))]
		[PXDefault(typeof(Search<INItemSiteSettings.preferredVendorID,
			Where<INItemSiteSettings.inventoryID, Equal<Current<SOLine.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Current<SOLine.siteID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<SOLine.siteID>))]
		public virtual int? VendorID { get; set; }
		#endregion
	}
}
