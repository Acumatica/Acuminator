using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.SO;

namespace PX.Objects.HackathonDemo.ReferentialIntegrity
{
	[PXCacheName("SO Order")]
	public class SOOrder : IBqlTable
	{
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXSelector(typeof(Search5<SOOrderType.orderType,
			InnerJoin<SOOrderTypeOperation, On2<SOOrderTypeOperation.FK.OrderType, And<SOOrderTypeOperation.operation, Equal<SOOrderType.defaultOperation>>>,
			LeftJoin<SOSetupApproval, On<SOOrderType.orderType, Equal<SOSetupApproval.orderType>>>>,
			Aggregate<GroupBy<SOOrderType.orderType>>>))]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }

		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		public abstract class orderNbr : IBqlField { }

		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXDBString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		public abstract class status : IBqlField { }

		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		[PXDefault]
		[CustomerActive(
			typeof(Search<BAccountR.bAccountID, Where<True, Equal<True>>>), // TODO: remove fake Where after AC-101187
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Customer.acctName),
			Filterable = true)]
		[PXForeignReference(typeof(Field<SOOrder.customerID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual int? CustomerID { get; set; }

		[PXDBTimestamp]
		public virtual byte[] tstamp { get; set; }
		public abstract class Tstamp : IBqlField { }
	}
}
