using PX.Data;
namespace PX.Objects.HackathonDemo
{
    public class AcctSubAttribute : PXAggregateAttribute
    {
        public bool IsDBField { get; set; } = true;
    }

    public class IIGPOALCLandedCost : IBqlTable
    {
        #region AcctSub
        public abstract class cost : PX.Data.IBqlField { }
        protected decimal? _Bound;
        [AcctSub]
        [PXDefault]
        [PXUIField(DisplayName = "Bound ")]
        public virtual decimal? Bound { get; set; }
        #endregion
    }
}