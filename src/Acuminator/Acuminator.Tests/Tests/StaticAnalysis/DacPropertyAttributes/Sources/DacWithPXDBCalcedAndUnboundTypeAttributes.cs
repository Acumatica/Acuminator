using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR;
using PX.Objects.GL;

using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacFieldWithDBCalcedAttribute.Sources
{
	public class Activity : IBqlTable
	{
		[PXDate]
		[PXDBCalced(
			typeof(Switch<
				Case<Where<lastIncomingActivityDate, IsNotNull, And<lastOutgoingActivityDate, IsNull>>,
					lastIncomingActivityDate,
				Case<Where<lastOutgoingActivityDate, IsNotNull, And<lastIncomingActivityDate, IsNull>>,
					lastOutgoingActivityDate,
				Case<Where<lastIncomingActivityDate, Greater<lastOutgoingActivityDate>>,
					lastIncomingActivityDate>>>,
				lastOutgoingActivityDate>),
			typeof(DateTime))]
		[PXUIField(DisplayName = "Last Activity Date", Enabled = false)]
		public virtual DateTime? LastActivityDate { get; set; }
		public abstract class lastActivityDate : IBqlField { }

		[PXDBDate]
		public virtual DateTime? LastIncomingActivityDate { get; set; }
		public abstract class lastIncomingActivityDate : IBqlField { }

		[PXDBDate]
		public virtual DateTime? LastOutgoingActivityDate { get; set; }
		public abstract class lastOutgoingActivityDate : IBqlField { }

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(255, IsUnicode = true, NonDB = true)]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(
			typeof(Switch<Case<Where<PaymentMethod.descr, IsNotNull>, PaymentMethod.descr>, CustomerPaymentMethod.descr>),
			typeof(string))]
		public virtual string Descr { get; set; }

		#endregion

		#region CalcRGOL
		public abstract class calcRGOL : PX.Data.BQL.BqlDecimal.Field<calcRGOL> { }

		[PXDecimal]
		[PXDBCalced(typeof(IIf<
			Where<ARTranPostGL.type, Equal<ARTranPost.type.application>>,
				ARTranPostGL.rGOLAmt,
				Zero>), typeof(decimal))]
		public virtual decimal? CalcRGOL { get; set; }
		#endregion

		#region MaxFinPeriodID
		public abstract class maxFinPeriodID : PX.Data.BQL.BqlString.Field<maxFinPeriodID> { }

		// Acuminator disable once PX1095 NoUnboundTypeAttributeWithPXDBCalced [Type field define with FinPeriod attribue]
		[FinPeriodID(IsDBField = false)]
		[PXDBCalced(typeof(IIf<Where<ARTranPostGL.type, Equal<ARTranPost.type.rgol>>
			, Null
			, ARTranPostGL.finPeriodID>), typeof(string))]
		public virtual string MaxFinPeriodID { get; set; }
		#endregion
	}
}
