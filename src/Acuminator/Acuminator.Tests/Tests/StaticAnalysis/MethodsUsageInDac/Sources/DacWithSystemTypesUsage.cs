using PX.Data;
using System;

namespace Acuminator.Tests.Sources
{
    public class ARSPCommissionPeriod : IBqlTable
    {
        [PXDate]
        [PXUIField(DisplayName = "From", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? StartDateUI
        {
            [PXDependsOnFields(typeof(startDate), typeof(endDate))]
            get
            {
                return (_StartDate != null && _EndDate != null && _StartDate == _EndDate) ? _StartDate.Value.AddDays(-1) : _StartDate;
            }
            set
            {
                _StartDate = (value != null && _EndDate != null && value == EndDateUI) ? value.Value.AddDays(1) : value;
            }
        }
        public abstract class startDateUI : IBqlField { }

        protected DateTime? _StartDate;
        [PXDBDate()]
        [PXDefault()]
        public virtual DateTime? StartDate
        {
            get
            {
                return this._StartDate.GetValueOrDefault();
            }
            set
            {
                this._StartDate = value;
            }
        }
        public abstract class startDate : PX.Data.IBqlField { }

        protected DateTime? _EndDate;
        [PXDBDate()]
        [PXDefault()]
        public virtual DateTime? EndDate
        {
            get
            {
                return this._EndDate;
            }
            set
            {
                this._EndDate = value;
            }
        }
        public abstract class endDate : PX.Data.IBqlField { }

        [PXDate()]
        [PXUIField(DisplayName = "To", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        public virtual DateTime? EndDateUI
        {
            [PXDependsOnFields(typeof(endDate))]
            get
            {
                return _EndDate?.AddDays(-1);
            }
            set
            {
                _EndDate = value?.AddDays(1);
            }
        }
        public abstract class endDateUI : PX.Data.IBqlField { }
    }
}
