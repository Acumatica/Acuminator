using PX.Data;
namespace PX.Objects.HackathonDemo
{
	public class AcctSubAttribute : PXAggregateAttribute
	{
		public bool IsDBField { get; set; } = true;
	}

	public class IIGPOALCLandedCost : IBqlTable
	{
		#region AcctSubBound
		public abstract class cost : PX.Data.IBqlField { }
		protected decimal? _Bound;
		[AcctSub(IsDBField = true)]
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
	}
}