using PX.Data;

namespace PX.Objects.HackathonDemo
{

	public class PeriodIDAttribute : PXAggregateAttribute
	{
		#region IsBDField
		public bool IsDBField { get; set; } = true;
		#endregion
	}

	public class IIGPOALCLandedCost : IBqlTable
	{

		#region Selected
		public abstract class selected : PX.Data.IBqlField { }
		protected bool? _Selected;
		[PeriodID(IsDBField = false)]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region Cost
		public abstract class cost : PX.Data.IBqlField { }
		protected decimal? _Cost;
		[PeriodID(IsDBField = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Cost")]
		public virtual decimal? Cost { get; set; }
		#endregion
	}
}