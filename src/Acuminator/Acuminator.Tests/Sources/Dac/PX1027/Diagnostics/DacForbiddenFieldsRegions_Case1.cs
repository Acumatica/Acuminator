using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public partial class SOOrder : IBqlTable
    {
        #region CompanyID         
        public abstract class companyId : IBqlField { }
        [PXDBString(IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Company ID")]
        public string CompanyID { get; set; }
        #endregion
        #region 2
        #endregion
        #region OrderNbr
        public abstract class orderNbr : IBqlField { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Order Nbr")]
        public int? OrderNbr { get; set; }
        #endregion
        #region CompanyID
        #endregion
    }
}
