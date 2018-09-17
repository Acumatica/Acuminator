using PX.Data;
namespace PX.Objects.HackathonDemo
{
    public class IIGPOALCLandedCost : IBqlTable
    {
        #region FieldUnbound1
        public abstract class selected : PX.Data.IBqlField { }
        protected bool? _Unbound1;
        [PeriodIDFalse]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Unbound 1")]
        public virtual bool? Unbound1
        {
            get
            {
                return _Unbound1;
            }
            set
            {
                _Unbound1 = value;
            }
        }
        #endregion
        #region FieldUnbound2
        public abstract class selected : PX.Data.IBqlField { }
        protected bool? _Unbound2;
        [AcctSubFalse]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Unbound 2")]
        public virtual bool? Unbound2
        {
            get
            {
                return _Unbound2;
            }
            set
            {
                _Unbound2 = value;
            }
        }
        #endregion
        #region FieldBound1
        public abstract class cost : PX.Data.IBqlField { }
        protected decimal? _Bound1;
        [PeriodID]
        [PXDefault]
        [PXUIField(DisplayName = "Bound 1")]
        public virtual decimal? Bound1 { get; set; }
        #endregion

        #region FieldBound2
        public abstract class cost : PX.Data.IBqlField { }
        protected decimal? _Bound2;
        [AcctSub]
        [PXDefault]
        [PXUIField(DisplayName = "Bound 2")]
        public virtual decimal? Bound2 { get; set; }
        #endregion
    }

    public class AcctSubAttribute : PXAggregateAttribute
    {
        public bool IsDBField { get; set; } = true;
    }
    public class PeriodIDAttribute : PXAggregateAttribute
    {
        protected bool _IsDBField = true;
        public bool IsDBField
        {
            get
            {
                return this._IsDBField;
            }
            set
            {
                this._IsDBField = value;
            }
        }
    }

    public class AcctSubFalseAttribute : PXAggregateAttribute
    {
        public bool IsDBField { get; set; } = false;
    }
    public class PeriodIDFalseAttribute : PXAggregateAttribute
    {
        protected bool _IsDBField = false;
        public bool IsDBField
        {
            get
            {
                return this._IsDBField;
            }
            set
            {
                this._IsDBField = value;
            }
        }
    }
}