using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.Objects.GL;
using PX.Objects.CS;

using CRLocation = PX.Objects.CR.Standalone.Location;
using GLBranchAttribute = PX.Objects.GL.BranchAttribute;

namespace PX.Objects.HackathonDemo.ReferentialIntegrity
{
	[PXCacheName("SO Order")]
	public class SOOrder : IBqlTable
	{
		#region OrderType
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXSelector(typeof(Search5<SOOrderType.orderType,
			InnerJoin<SOOrderTypeOperation, On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
			LeftJoin<SOSetupApproval, On<SOOrderType.orderType, Equal<SOSetupApproval.orderType>>>>,
			Aggregate<GroupBy<SOOrderType.orderType>>>))]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }

		public abstract class orderType : IBqlField { }
		#endregion

		#region OrderNbr
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }

		public abstract class orderNbr : IBqlField { }
		#endregion

		#region Status
		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXDBString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }

		public abstract class status : IBqlField { }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[GLBranch(typeof(Coalesce<
			Search<Location.cBranchID, Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
				And<Location.locationID, Equal<Current<SOOrder.customerLocationID>>>>>,
			Search<Branch.branchID, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>))]
		public virtual int? BranchID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDefault]
		[CustomerActive(
			typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Customer.acctName),
			Filterable = true)]
		[PXForeignReference(typeof(Field<SOOrder.customerID>.IsRelatedTo<BAccount.bAccountID>))]

		public virtual int? CustomerID { get; set; }
		#endregion

		#region CustomerLocationID
		public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<Location.isActive, Equal<True>,
			And<MatchWithBranch<Location.cBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
			InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
			Where<BAccountR.bAccountID, Equal<Current<SOOrder.customerID>>,
				And<CRLocation.isActive, Equal<True>,
				And<MatchWithBranch<CRLocation.cBranchID>>>>>,
			Search<CRLocation.locationID,
			Where<CRLocation.bAccountID, Equal<Current<SOOrder.customerID>>,
			And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.cBranchID>>>>>>))]
		[PXForeignReference(
			typeof(CompositeKey<
				Field<SOOrder.customerID>.IsRelatedTo<Location.bAccountID>,
				Field<SOOrder.customerLocationID>.IsRelatedTo<Location.locationID>
			>))]
		public virtual int? CustomerLocationID { get; set; }
		#endregion

		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? OrderDate { get; set; }
		#endregion

		#region Tstamp
		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }

		public abstract class Tstamp : IBqlField { }
		#endregion
	}
}
