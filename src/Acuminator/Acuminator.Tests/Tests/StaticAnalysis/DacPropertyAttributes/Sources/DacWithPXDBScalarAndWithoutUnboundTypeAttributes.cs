using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR;
using PX.Objects.GL;

using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacFieldWithDBCalcedAttribute.Sources
{
	public class Activity : IBqlTable
	{
		#region OffsetCashAccountID
		public abstract class offsetCashAccountID : PX.Data.BQL.BqlInt.Field<offsetCashAccountID> { }


		[PXDBScalar(typeof(Search<CashAccount.cashAccountID,
				Where<CashAccount.accountID, Equal<CashAccountETDetail.offsetAccountID>,
				And<CashAccount.subID, Equal<CashAccountETDetail.offsetSubID>,
				And<CashAccount.branchID, Equal<CashAccountETDetail.offsetBranchID>>>>>))]
		public virtual int? OffsetCashAccountID
		{
			get;
			set;
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBScalar(
			typeof(Search<PaymentMethod.descr>))]
		public virtual string Descr { get; set; }

		#endregion

		#region CalcRGOL
		public abstract class calcRGOL : PX.Data.BQL.BqlDecimal.Field<calcRGOL> { }

		[PXDBScalar(
			typeof(Search<ARTranPostGL.rGOLAmt>))]
		public virtual decimal? CalcRGOL { get; set; }
		#endregion

		#region MaxFinPeriodID
		public abstract class maxFinPeriodID : PX.Data.BQL.BqlString.Field<maxFinPeriodID> { }

		[FinPeriodID]
		[PXDBScalar(
			typeof(Search<ARTranPostGL.finPeriodID>))]
		public virtual string MaxFinPeriodID { get; set; }
		#endregion
	}
}