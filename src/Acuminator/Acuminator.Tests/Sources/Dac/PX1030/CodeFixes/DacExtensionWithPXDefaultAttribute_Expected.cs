using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public class SOOrderWithTotal : PXCacheExtension<SOOrder>
    {
        #region TotalAmount
        public abstract class totalAmount : IBqlField { }

        [PXDBDecimal]
        [PXUIField(DisplayName = "Total")]
        public decimal TotalAmount { get; set; }
        #endregion

        #region UsrNPCADocLineCntr
        public abstract class usrNPCADocLineCntr : IBqlField { }
        [PXDBInt]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? UsrNPCADocLineCntr { get; set; }
        #endregion
    }
}