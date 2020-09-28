using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR;

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
	}
}
