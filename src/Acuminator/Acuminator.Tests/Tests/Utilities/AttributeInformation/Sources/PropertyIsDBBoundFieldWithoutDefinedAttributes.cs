using PX.Data;
namespace PX.Objects.HackathonDemo
{
	public class IIGPOALCLandedCost : IBqlTable
	{
		#region AcctSubTrue
		public abstract class cost : PX.Data.IBqlField { }
		protected decimal? _Bound;
		[AcctSubBound]
		[PXDefault]
		[PXUIField(DisplayName = "Bound")]
		public virtual decimal? Bound { get; set; }
		#endregion
		#region AcctSubFalse
		public abstract class cost : PX.Data.IBqlField { }
		protected decimal? Unbound;
		[AcctSubUnbound]
		[PXDefault]
		[PXUIField(DisplayName = "Unbound")]
		public virtual decimal? Unbound { get; set; }
		#endregion

		[PXDependsOnFields(typeof(Bound), typeof(Unbound))] // doesn't inherit from PXEventSubscriberAttribute, so BoundType will be Unknown
		[PXDBDecimal] // BoundType.DbBound
		public decimal? Total => Bound * Unbound;
	}
}