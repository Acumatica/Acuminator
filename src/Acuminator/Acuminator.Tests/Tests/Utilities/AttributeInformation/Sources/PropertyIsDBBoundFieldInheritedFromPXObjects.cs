using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.HackathonDemo
{
	public class IIGPOALCLandedCost : IBqlTable
	{
		#region AcctSubBound
		public abstract class cost : PX.Data.IBqlField { }
		protected decimal? _Bound;
		[AcctSub]
		[PXDefault]
		[PXUIField(DisplayName = "Bound ")]
		public virtual decimal? Bound { get; set; }
		#endregion
		#region AcctSubUnbound
		public abstract class cost : PX.Data.IBqlField { }
		protected decimal? _Unbound;
		[AcctSub(IsDBField = false)]
		[PXDefault]
		[PXUIField(DisplayName = "Unbound ")]
		public virtual decimal? Unbound { get; set; }
		#endregion

		#region PeriodID attribute

		#region Bound

		public abstract class periodIDBound : PX.Data.IBqlField {}
		protected decimal? _PeriodIDBound;

		[PeriodID]
		[PXDefault]
		[PXUIField(DisplayName = "Bound Period ID")]
		public virtual decimal? PeriodIDBound { get; set; }

		#endregion

		#region Unbound

		public abstract class periodIDUnbound : PX.Data.IBqlField {}
		protected decimal? _PeriodIDUnbound;

		[PeriodID(IsDBField = false)]
		[PXDefault]
		[PXUIField(DisplayName = "Unbound PeriodID")]
		public virtual decimal? PeriodIDUnbound { get; set; }

		#endregion

		#endregion


	}
}