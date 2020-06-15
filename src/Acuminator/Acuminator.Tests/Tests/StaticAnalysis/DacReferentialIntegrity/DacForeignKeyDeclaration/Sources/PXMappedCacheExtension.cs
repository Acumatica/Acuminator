using System;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.Sources
{
    public class Adjust : PXMappedCacheExtension
    {
        #region AdjgBranchID
        public abstract class adjgBranchID : PX.Data.IBqlField { }

        public virtual int? AdjgBranchID { get; set; }
        #endregion
        #region AdjgFinPeriodID
        public abstract class adjgFinPeriodID : PX.Data.IBqlField { }

        public virtual String AdjgFinPeriodID { get; set; }
        #endregion
        #region AdjgTranPeriodID
        public abstract class adjgTranPeriodID : PX.Data.IBqlField { }

        public virtual String AdjgTranPeriodID { get; set; }
        #endregion
        #region AdjdBranchID
        public abstract class adjdBranchID : PX.Data.IBqlField { }

        public virtual int? AdjdBranchID { get; set; }
        #endregion
        #region AdjdFinPeriodID
        public abstract class adjdFinPeriodID : PX.Data.IBqlField { }

        public virtual String AdjdFinPeriodID { get; set; }
        #endregion
        #region AdjdTranPeriodID
        public abstract class adjdTranPeriodID : PX.Data.IBqlField { }

        public virtual String AdjdTranPeriodID { get; set; }
        #endregion
    }
}
