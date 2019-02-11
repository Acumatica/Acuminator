using PX.Data;
using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacFieldWithDBCalcedAttribute.Sources
{
	public class Activity : IBqlTable
	{
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
	}
}
